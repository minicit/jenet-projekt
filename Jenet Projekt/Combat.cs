﻿using Jenet_Projekt.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Jenet_Projekt
{
    class Combat
    {
        private SpriteHelper spriteHelper = new SpriteHelper();
        private bool combatActive, playerRan;
        private GameEntity enemy, player;
        private int turn; //zug variable; 1:spieler, 2:Gegner, 3:Kampfende?
        private Random rand = new Random();
        private ProgressBar playerbar, enemybar;
        private Eventcaller subject;
        private Panel combatPanel;
        private int[] itemArray;
        private bool stuffHappening = false;
        public void begin( GameEntity emy, GameEntity ply, Panel combatPanel, Eventcaller subject, ProgressBar playerbar, ProgressBar enemybar, int[] itemArray)
        {
            playerRan = false;
            this.enemy = emy;
            this.player = ply;
            this.playerbar = playerbar;
            this.enemybar = enemybar;
            this.subject = subject;
            this.combatPanel = combatPanel;
            this.itemArray = itemArray;
            combatActive = true;
            Graphics g = combatPanel.CreateGraphics();
            combatPanel.BackgroundImage = spriteHelper.getBackground(-1);
            combatPanel.Show();

            playerbar.Maximum = (int)player.getMaxHealth();
            enemybar.Maximum = (int)enemy.getMaxHealth();
            if ((int)player.getHealth() < 0)
            {
                playerbar.Value = 0;
            }
            else
            {
                playerbar.Value = (int)player.getHealth();
            }
            enemybar.Value = (int)enemy.getHealth();
            drawFight(g, enemy, player);
            g.Dispose();
        }

        public bool getStuff() { return stuffHappening; }

        public void useItem(int type)
        {
            if (itemArray[type] != 0)
            {
                switch (type)
                {
                    case 0:
                        MessageBox.Show("Mundschutz");
                        fightSprite(Resources.Resource1.Mundschutz);
                        Main.getInstance().addtoList("Enemy attack has been lowered");
                        enemy.setAttack(5);
                        break;
                    case 1:
                        MessageBox.Show("Feuerzeug");
                        fightSprite(Resources.Resource1.Desinfektionsmittel_Feuerzeug);
                        enemy.takeDamage(40);
                        break;
                    case 2:
                        MessageBox.Show("Spritze");
                        fightSprite(Resources.Resource1.spritze_Item);
                        Main.getInstance().addtoList("Enemy attack has been lowered");
                        enemy.setAttack(1);
                        break;
                    case 3:
                        MessageBox.Show("Croc");
                        fightSprite(Resources.Resource1.Crocs);
                        if (enemy.getClass() == Klasse.Klassen.Bat)
                        {
                            Main.getInstance().addtoList("Corona wurde mit dem Schuh kaputt gehauen");
                            enemy.setHealth(0);
                        }
                        else
                        {
                            Main.getInstance().addtoList("Nothing happened");
                        }
                        break;
                }
                itemArray[type] -= 1;
                combatOverCheck();
            }
            Main.getInstance().showItems(itemArray);
        }

        private void drawFight(Graphics g, GameEntity enemy, GameEntity player)
        {
            g.DrawImage(spriteHelper.getCombatSprite(player.getClass(), player.getClass()), 170, 600);
            if (enemy.getClass() == Klasse.Klassen.Bat)
            {
                g.DrawImage(spriteHelper.getCombatSprite(enemy.getClass(), player.getClass()), 950-300, 0);
            }
            else
            {
                g.DrawImage(spriteHelper.getCombatSprite(enemy.getClass(),player.getClass()), 950, 150);
            }
        }

        private void enemyAction()
        {
            if(((enemy.getHealth()/enemy.getMaxHealth())*90) > rand.Next(100))
            {
                enemyShield();
            }
            else
            {
                enemyAttack();
            }
        }

        private void fightSprite(Bitmap sprite)
        {
            Graphics g = combatPanel.CreateGraphics();
            g.DrawImage(sprite, 600, 400);
            stuffHappening = true;
            System.Threading.Thread.Sleep(1000);
            stuffHappening = false;
            g.DrawImage(spriteHelper.getBackground(-1), new Rectangle(600, 400, sprite.Width, sprite.Height), new Rectangle(476, 200, sprite.Width, sprite.Height), GraphicsUnit.Point);
        }

        public bool getCombatActive() { return combatActive; }

        public void setCombatActive(bool active) { combatActive = active; }

        public void setTurn(int turn) { this.turn = turn; }

        public int getTurn() { return turn; }

        private void enemyAttack()
        {
            enemy.setShield(false);
            if (enemy.doesItHit(enemy, player))
            {
                player.takeDamageFrom(enemy);
                if (player.getHealth() > 0)
                    playerbar.Value = (int)player.getHealth();
                else
                    playerbar.Value = 0;
                fightSprite(Resources.Resource1.OOF);
            }
            else
            {
                fightSprite(Resources.Resource1.Missed);
            }
            
        }

        private void enemyShield()
        {
            enemy.setShield(true);
            fightSprite(Resources.Resource1.Schild);
        }

        public void attack()
        {
            if (player.getSpeed() >= enemy.getSpeed())
            {
                player.setShield(false);
                if (player.doesItHit(player, enemy))
                {
                    enemy.takeDamageFrom(player);
                    if (enemy.getHealth() > 0)
                        enemybar.Value = (int)enemy.getHealth();
                    else
                        enemybar.Value = 0;
                    fightSprite(Resources.Resource1.Sprite_Pow);
                }
                else
                {
                    fightSprite(Resources.Resource1.Missed);
                }
                enemyAction();
                enemy.setShield(false);
            }
            else
            {
                enemyAction();
                player.setShield(false);
                if (player.doesItHit(player, enemy))
                {
                    enemy.takeDamageFrom(player);
                    if (enemy.getHealth() > 0)
                    {
                        try { enemybar.Value = (int)enemy.getHealth(); }
                        catch (Exception e) 
                        {
                            enemybar.Value = enemybar.Maximum;
                        }
                    }
                    fightSprite(Resources.Resource1.Sprite_Pow);
                }
                else
                {
                    fightSprite(Resources.Resource1.Missed);
                }
                enemy.setShield(false);
            }
            combatOverCheck();
        }

        public void shield()
        {
            player.setShield(true);
            fightSprite(Resources.Resource1.Schild);
        }

        public void items()
        {
            using (var form = new UseItem(itemArray)){
                form.ShowDialog();
                useItem(form.returnvalue);
            }

            player.setShield(false);
        }

        public void run()
        {
            player.setShield(false);
            if (player.doesItHit(player, enemy))
            {
                playerRan = true;
                fightSprite(Resources.Resource1.Desinfektionsmittel);
            }
            else
            {
                fightSprite(Resources.Resource1.OOF);
                enemyAction();
            }
            combatOverCheck();
        }

        private void combatOverCheck()
        {
            if (enemy.getHealth() <= 0)
            {
                setCombatActive(false);
                subject.PlayerWon(player);
            }
            else if (player.getHealth() <= 0)
            {
                setCombatActive(false);
                subject.EnemyWon(enemy);
            }
            else if (playerRan)
            {
                setCombatActive(false);
                subject.PlayerRan();
                playerRan = false;
            }

        }
    }
}
