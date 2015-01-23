﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProgramonEngine;
using System;
using System.Collections.Generic;
using XnaGuiItems.Core;
using XnaGuiItems.Items;
using Button = XnaGuiItems.Items.Button;
using GuiItemCollection = XnaGuiItems.Core.GuiItem.GuiItemCollection;

namespace Programon
{
    public class BattleScreen
    {
        public bool RequestNextStage { get; set; }

        public bool Loaded { get; private set; }
        public bool Initialized { get; private set; }

        public int Focus
        {
            get { return focus; }
            set
            {
                if (focus < 0) value = attacking ? FocusTargets.Count : FocusTargets.Count - 3;
                else if (focus > (attacking ? FocusTargets.Count : FocusTargets.Count - 3)) value = 0;
                focus = value;

                if (FocusTargets[focus] != FocusItem)
                {
                    (FocusTargets[focus] as Label).Text = "|>" + (FocusTargets[focus] as Label).Text;
                    if (FocusItem != null) (FocusItem as Label).Text = (FocusItem as Label).Text.Replace("|>", "");
                    FocusItem = FocusTargets[focus];
                    FocusItem.Position -= new Vector2(10, 0);
                }
            }
        }

        private GuiItemCollection EnemyChilds;
        public GuiItemCollection PlayerChilds;
        private GuiItemCollection AttacksChilds;
        private GuiItemCollection OptionsChilds;
        private List<GuiItem> FocusTargets;
        private GuiItem FocusItem;

        private Player player;
        public Creature curProgramon;
        public Creature enemy;

        private GraphicsDevice device;
        private Rectangle bufferSize;
        private bool attacking;
        private int focus;

        public BattleScreen(Player player, GraphicsDevice device, Rectangle bufferSize, Creature enemy)
        {
            this.player = player;
            this.enemy = enemy;
            this.device = device;
            this.bufferSize = bufferSize;
            curProgramon = player.programons[0];
        }

        public void InitializeEvents(MouseEventHandler onProgramonClick, MouseEventHandler onItemsClick, MouseEventHandler onRunClick)
        {
            if (!Loaded) throw new InvalidOperationException("Load must be called before InitializeEvents!");

            OptionsChilds["OptionsAttack"].Click += OnAttackClick;
            OptionsChilds["OptionsAttackName"].Click += OnAttackClick;

            if (onProgramonClick != null)
            {
                OptionsChilds["OptionsProgramon"].Click += onProgramonClick;
                OptionsChilds["OptionsProgramonName"].Click += onProgramonClick;
            }

            if (onItemsClick != null)
            {
                OptionsChilds["OptionsItems"].Click += onItemsClick;
                OptionsChilds["OptionsItemsName"].Click += onItemsClick;
            }

            if (onRunClick != null)
            {
                OptionsChilds["OptionsRun"].Click += onRunClick;
                OptionsChilds["OptionsRunName"].Click += onRunClick;
            }

            AttacksChilds.ForEach(i =>
            {
                i.Click += (sender, e) =>
                {
                    switch ((sender as GuiItem).Name)
                    {
                        case ("Attack1"):
                            if (curProgramon.abilities[0].EP > 0)
                            {
                                curProgramon.abilities[0].EP--;
                                BattleMechanics.PlayerAttack(ref curProgramon, ref enemy, curProgramon.abilities[0]);
                                Reload();
                            }
                            break;
                        case ("Attack2"):
                            if (curProgramon.abilities[1].EP > 0)
                            {
                                curProgramon.abilities[1].EP--;
                                BattleMechanics.PlayerAttack(ref curProgramon, ref enemy, curProgramon.abilities[1]);
                                Reload();
                            }
                            break;
                        case ("Attack3"):
                            if (curProgramon.abilities[2].EP > 0)
                            {
                                curProgramon.abilities[2].EP--;
                                BattleMechanics.PlayerAttack(ref curProgramon, ref enemy, curProgramon.abilities[2]);
                                Reload();
                            }
                            break;
                    }

                    if (enemy.programonBaseStats.health <= 0 || curProgramon.programonBaseStats.health <= 0) return;

                    BattleMechanics.ComputerAttack(ref curProgramon, ref enemy);
                    RequestNextStage = true;
                };
            });

            Initialized = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            EnemyChilds.Owner.Draw(spriteBatch);
            EnemyChilds.ForEach(i => i.Draw(spriteBatch));

            PlayerChilds.Owner.Draw(spriteBatch);
            PlayerChilds.ForEach(i => i.Draw(spriteBatch));

            if (attacking)
            {
                AttacksChilds.Owner.Draw(spriteBatch);
                AttacksChilds.ForEach(i => i.Draw(spriteBatch));
            }

            OptionsChilds.Owner.Draw(spriteBatch);
            OptionsChilds.ForEach(i => i.Draw(spriteBatch));
        }

        public bool Update(float deltaTime)
        {
            MouseState ms = Mouse.GetState();

            curProgramon.UpdateBuffDuration();
            enemy.UpdateBuffDuration();

            (PlayerChilds["PlayerProgramonHealth"] as Label).Text = string.Format("{0}/{1} HP", curProgramon.programonBaseStats.health.ToString(), curProgramon.programonBaseStats.maxHealth);
            (PlayerChilds["PlayerProgramonHealthBar"] as ProgresBar).Value = GetProgresBarValue(curProgramon.programonBaseStats.health, curProgramon.programonBaseStats.maxHealth);

            (PlayerChilds["PlayerProgramonXp"] as Label).Text = string.Format("{0}/{1} XP", curProgramon.exp.ToString(), (int)(Math.Pow(curProgramon.level, 1.337) * LevelingSystem.startingExp));
            (PlayerChilds["PlayerProgramonXpBar"] as ProgresBar).Value = GetProgresBarValue(curProgramon.exp, (int)Math.Pow(curProgramon.level, 1.337) * LevelingSystem.startingExp);

            (EnemyChilds["EnemyProgramonHealth"] as Label).Text = string.Format("{0}/{1} HP", enemy.programonBaseStats.health.ToString(), enemy.programonBaseStats.maxHealth);
            (EnemyChilds["EnemyProgramonHealthBar"] as ProgresBar).Value = GetProgresBarValue(enemy.programonBaseStats.health, enemy.programonBaseStats.maxHealth);

            EnemyChilds.Owner.Update(ms);
            EnemyChilds.ForEach(i => Update(i, ms, deltaTime));

            PlayerChilds.Owner.Update(ms);
            PlayerChilds.ForEach(i => Update(i, ms, deltaTime));

            if (attacking)
            {
                AttacksChilds.Owner.Update(ms);
                AttacksChilds.ForEach(i => Update(i, ms, deltaTime));
            }

            OptionsChilds.Owner.Update(ms);
            OptionsChilds.ForEach(i => Update(i, ms, deltaTime));

            if (RequestNextStage)
            {
                RequestNextStage = false;
                attacking = false;
            }

            return enemy.programonBaseStats.health <= 0 || curProgramon.programonBaseStats.health <= 0 ? true : false;
        }

        public void Load(ContentManager content, string fontName, string smallFontName)
        {
            

            SpriteFont font = content.Load<SpriteFont>(fontName);
            SpriteFont smallFont = content.Load<SpriteFont>(smallFontName);

            FocusTargets = new List<GuiItem>();

            #region Options
            GuiItem optionsBase = new GuiItem(device, new Rectangle(bufferSize.Width >> 1, bufferSize.Height - (int)(bufferSize.Height / 7.5f), bufferSize.Width >> 1, bufferSize.Height / 6));
            OptionsChilds = new GuiItemCollection(optionsBase);

            OptionsChilds.Add(new Button(device, new Rectangle(0, 0, optionsBase.Bounds.Width >> 2, optionsBase.Bounds.Height), smallFont) { Name = "OptionsAttack" });
            OptionsChilds.Add(new Label(device, new Rectangle(optionsBase.Bounds.Width >> 4, optionsBase.Bounds.Height >> 2, 1, 1), smallFont) { AutoSize = true, Name = "OptionsAttackName", Text = "Attack" });
            FocusTargets.Add(OptionsChilds["OptionsAttackName"]);

            OptionsChilds.Add(new Button(device, new Rectangle((optionsBase.Bounds.Width >> 2) + 1, 0, optionsBase.Bounds.Width >> 2, optionsBase.Bounds.Height), smallFont) { Name = "OptionsProgramon" });
            OptionsChilds.Add(new Label(device, new Rectangle(((optionsBase.Bounds.Width >> 2) + 1) + (optionsBase.Bounds.Width >> 6), optionsBase.Bounds.Height >> 2, 1, 1), smallFont) { AutoSize = true, Name = "OptionsProgramonName", Text = "Programon" });
            FocusTargets.Add(OptionsChilds["OptionsProgramonName"]);

            OptionsChilds.Add(new Button(device, new Rectangle((optionsBase.Bounds.Width >> 1) + 2, 0, optionsBase.Bounds.Width >> 2, optionsBase.Bounds.Height), smallFont) { Name = "OptionsItems" });
            OptionsChilds.Add(new Label(device, new Rectangle(((optionsBase.Bounds.Width >> 1) + 2) + (optionsBase.Bounds.Width >> 4), optionsBase.Bounds.Height >> 2, 1, 1), smallFont) { AutoSize = true, Name = "OptionsItemsName", Text = "Items" });
            FocusTargets.Add(OptionsChilds["OptionsItemsName"]);

            OptionsChilds.Add(new Button(device, new Rectangle((optionsBase.Bounds.Width - (optionsBase.Bounds.Width >> 2)) + 3, 0, optionsBase.Bounds.Width >> 2, optionsBase.Bounds.Height), smallFont) { Name = "OptionsRun" });
            OptionsChilds.Add(new Label(device, new Rectangle(((optionsBase.Bounds.Width - (optionsBase.Bounds.Width >> 2)) + 3) + (optionsBase.Bounds.Width >> 4), optionsBase.Bounds.Height >> 2, 1, 1), smallFont) { AutoSize = true, Name = "OptionsRunName", Text = "Run" });
            FocusTargets.Add(OptionsChilds["OptionsRunName"]);
            #endregion

            #region EnemyStatusBox
            GuiItem enemyBase = new GuiItem(device, new Rectangle(bufferSize.X, bufferSize.Y, bufferSize.Width >> 1, bufferSize.Height >> 3));
            EnemyChilds = new GuiItemCollection(enemyBase);

            EnemyChilds.Add(new Label(device, new Rectangle(0, 0, 1, 1), font) { AutoSize = true, Name = "EnemyProgramonName", Text = enemy.name, BackColor = Color.Transparent });

            Vector2 programonLvlVect2 = font.MeasureString(string.Format("Lvl: {0}", enemy.level));
            EnemyChilds.Add(new Label(device, new Rectangle(enemyBase.Bounds.Width - programonLvlVect2.X(), 0, 1, 1), font) { AutoSize = true, Name = "EnemyProgramonLvl", Text = "Lvl: " + enemy.level, BackColor = Color.Transparent });

            EnemyChilds.Add(new Label(device, new Rectangle(0, enemyBase.Bounds.Height >> 1, 1, 1), font) { AutoSize = true, Name = "EnemyProgramonHealth", Text = string.Format("{0}/{1} HP", enemy.programonBaseStats.health, enemy.programonBaseStats.maxHealth), BackColor = Color.Transparent });

            EnemyChilds.Add(new ProgresBar(device, new Rectangle(0, (enemyBase.Bounds.Height >> 1) - (font.MeasureString("a").Y() >> 2), enemyBase.Bounds.Width, 15)) { Name = "EnemyProgramonHealthBar", ForeColor = Color.Red, BackColor = Color.LightGray });
            (EnemyChilds["EnemyProgramonHealthBar"] as ProgresBar).Value = GetProgresBarValue(enemy.programonBaseStats.health, enemy.programonBaseStats.maxHealth);

            //Status Icon
            #endregion

            #region PlayerStatusBox
            GuiItem playerBase = new GuiItem(device, new Rectangle(bufferSize.Width >> 1, bufferSize.Height - (int)(bufferSize.Height / 3f), bufferSize.Width >> 1, bufferSize.Height / 5));
            PlayerChilds = new GuiItemCollection(playerBase);

            PlayerChilds.Add(new Label(device, new Rectangle(0, 0, 1, 1), font) { AutoSize = true, Name = "PlayerProgramonName", Text = curProgramon.name, BackColor = Color.Transparent });

            programonLvlVect2 = font.MeasureString(string.Format("Lvl: {0}", enemy.level));
            PlayerChilds.Add(new Label(device, new Rectangle(playerBase.Bounds.Width - programonLvlVect2.X(), 0, 1, 1), font) { AutoSize = true, Name = "PlayerProgramonLvl", Text = "Lvl: " + curProgramon.level, BackColor = Color.Transparent });

            PlayerChilds.Add(new Label(device, new Rectangle(0, (playerBase.Bounds.Height >> 1) - (font.MeasureString("a").Y() >> 2), 1, 1), font) { AutoSize = true, Name = "PlayerProgramonHealth", Text = string.Format("{0}/{1} HP", curProgramon.programonBaseStats.health, curProgramon.programonBaseStats.maxHealth), BackColor = Color.Transparent });

            PlayerChilds.Add(new ProgresBar(device, new Rectangle(0, (playerBase.Bounds.Height >> 1) - (font.MeasureString("a").Y() >> 1), playerBase.Bounds.Width, 15)) { Name = "PlayerProgramonHealthBar", ForeColor = Color.Red, BackColor = Color.LightGray });
            (PlayerChilds["PlayerProgramonHealthBar"] as ProgresBar).Value = GetProgresBarValue(curProgramon.programonBaseStats.health, curProgramon.programonBaseStats.maxHealth);

            PlayerChilds.Add(new Label(device, new Rectangle(0, (playerBase.Bounds.Height >> 1) + PlayerChilds["PlayerProgramonHealth"].Bounds.Height, 1, 1), font) { AutoSize = true, Name = "PlayerProgramonXp", Text = string.Format("{0}/{1} XP", curProgramon.exp, Math.Pow(curProgramon.level, 1.337) * LevelingSystem.startingExp), BackColor = Color.Transparent });

            PlayerChilds.Add(new ProgresBar(device, new Rectangle(0, (playerBase.Bounds.Height >> 1) + PlayerChilds["PlayerProgramonHealth"].Bounds.Height - (font.MeasureString("a").Y() >> 2), playerBase.Bounds.Width, 15)) { Name = "PlayerProgramonXpBar", ForeColor = Color.DeepSkyBlue, BackColor = Color.LightGray });
            (PlayerChilds["PlayerProgramonXpBar"] as ProgresBar).Value = GetProgresBarValue(curProgramon.exp, (int)Math.Pow(curProgramon.level, 1.337) * LevelingSystem.startingExp);

            //Status Icon
            #endregion

            #region Attacks
            GuiItem attacksBase = new GuiItem(device, new Rectangle(0, bufferSize.Height - (int)(bufferSize.Height / 7.5f), bufferSize.Width >> 1, bufferSize.Height / 6));
            AttacksChilds = new GuiItemCollection(attacksBase);

            AttacksChilds.Add(new Button(device, new Rectangle(0, 0, attacksBase.Bounds.Width / 3, attacksBase.Bounds.Height), smallFont) { Name = "Attack1" });
            AttacksChilds.Add(new Label(device, new Rectangle(1, attacksBase.Bounds.Height >> 2, 1, 1), smallFont) { AutoSize = true, Name = "Attack1Name", Text = curProgramon.abilities[0].name });
            AttacksChilds.Add(new Label(device, new Rectangle(attacksBase.Bounds.Width >> 3, attacksBase.Bounds.Height >> 1, 1, 1), smallFont) { AutoSize = true, Name = "Attack1Ep", Text = string.Format("{0}/{1}EP", curProgramon.abilities[0].EP, curProgramon.abilities[0].maxEP) });
            FocusTargets.Add(AttacksChilds["Attack1Name"]);

            AttacksChilds.Add(new Button(device, new Rectangle(attacksBase.Bounds.Width / 3 + 1, 0, attacksBase.Bounds.Width / 3, attacksBase.Bounds.Height), smallFont) { Name = "Attack2" });
            AttacksChilds.Add(new Label(device, new Rectangle(2 + (attacksBase.Bounds.Width / 3), attacksBase.Bounds.Height >> 2, 1, 1), smallFont) { AutoSize = true, Name = "Attack2Name", Text = curProgramon.abilities[1].name });
            AttacksChilds.Add(new Label(device, new Rectangle((attacksBase.Bounds.Width / 3 + 1) + (attacksBase.Bounds.Width >> 3), attacksBase.Bounds.Height >> 1, 1, 1), smallFont) { AutoSize = true, Name = "Attack2Ep", Text = string.Format("{0}/{1}EP", curProgramon.abilities[1].EP, curProgramon.abilities[1].maxEP) });
            FocusTargets.Add(AttacksChilds["Attack2Name"]);

            AttacksChilds.Add(new Button(device, new Rectangle((attacksBase.Bounds.Width / 3 + 1) * 2, 0, attacksBase.Bounds.Width / 3, attacksBase.Bounds.Height), smallFont) { Name = "Attack3" });
            AttacksChilds.Add(new Label(device, new Rectangle(3 + ((attacksBase.Bounds.Width / 3) * 2), attacksBase.Bounds.Height >> 2, 1, 1), smallFont) { AutoSize = true, Name = "Attack3Name", Text = curProgramon.abilities[2].name });
            AttacksChilds.Add(new Label(device, new Rectangle(2 + ((attacksBase.Bounds.Width / 3) * 2) + (attacksBase.Bounds.Width >> 3), attacksBase.Bounds.Height >> 1, 1, 1), smallFont) { AutoSize = true, Name = "Attack3Ep", Text = string.Format("{0}/{1}EP", curProgramon.abilities[2].EP, curProgramon.abilities[2].maxEP) });
            FocusTargets.Add(AttacksChilds["Attack3Name"]);
            #endregion

            Focus = 0;
            Loaded = true;
        }

        private void Reload()
        {
            (AttacksChilds["Attack1Ep"] as Label).Text = curProgramon.abilities[0].EP.ToString() + "/" +curProgramon.abilities[0].maxEP.ToString() + "EP";
            (AttacksChilds["Attack2Ep"] as Label).Text = curProgramon.abilities[1].EP.ToString()+ "/" +curProgramon.abilities[1].maxEP.ToString() + "EP";
            (AttacksChilds["Attack3Ep"] as Label).Text = curProgramon.abilities[2].EP.ToString()+ "/" +curProgramon.abilities[2].maxEP.ToString() + "EP";
        }

        private int GetProgresBarValue(int value, int maxValue)
        {
            float curToOne = (float)value / (float)maxValue;
            float curToMax = curToOne * 100f;
            return (int)curToMax;
        }

        private void OnAttackClick(object sender, MouseState e) { attacking = true; }

        private void Update(GuiItem item, MouseState ms, float dt)
        {
            if (item.GetType() == typeof(Button)) (item as Button).Update(ms, dt);
            else item.Update(ms);
        }
    }
}