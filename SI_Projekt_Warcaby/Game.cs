﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Warcaby
{
    class Game // note : narazie nie zaimplementowałem jeszcze "damek". Obsługa sterowania: Klikamy na pionek (zaczyna czerwony), następnie na pole. Jeszcze dorobię jakieś oznaczenia, ze wybrany 
    {
        //stany gry w których moze być gracz
        private enum State
        {
            selecting_checker, //wybór pionka
            selecting_field,    //wybór pola na które ma przejść pionek
            selecting_field_series  //"kombo" zabójstw
        }

        public class Checker
        {
            public Checker(int owner)
            {
                this.owner = owner;
                this.isKing = false;
            }
            public bool isKing;
            public int owner;
        }
        private abstract class Player
        {
            protected bool side;

            protected Game game;

            public Player(Game g)
            {
                game = g;
            }
        };

        private class HumanPlayer : Player
        {
            private State currentState;

            private Point selectedCheckerField;

            public HumanPlayer(Game g)
                :base(g)
            {

            }

            public void ProcessInputLeftButton(Point selectedField)//w zależnosci od stanu tury gracza przetwarzamy działanie lewego przycisku
            {
                switch(currentState)       
                {
                    case State.selecting_checker:       //wybór pionka - chwilowo brak możliwości odznaczenia i zaznaczenia innego - todo
                        if(game.board[selectedField.Y * game.size + selectedField.X]!=null &&
                            game.board[selectedField.Y*game.size+selectedField.X].owner==game.currentPlayer)
                        {
                            selectedCheckerField = selectedField;
                            currentState = State.selecting_field;
                        }
                        break;

                    case State.selecting_field: //wybór kolejnego pola planszy na ktore ma stanąć pionek
                        List<Point> l = game.getPossibleMoves(selectedCheckerField.X, selectedCheckerField.Y);
                            foreach (Point p in l)
                        {
                            if(p.Equals(selectedField))     //jedynie z możliwych do wyboru pól zwracanych do listy l
                            {
                                game.MoveChecker(selectedCheckerField, selectedField);
                                game.nextPlayer();
                                currentState = State.selecting_checker;
                            }
                        }
                        break;
                }
            }
        }

        Player[] players = new Player[2];

        int currentPlayer = 0;

        private Checker[] board; // plansza ma -1 w miejscach braku pionka. Pionki gracza 0 mają indeks 0, a gracza 1 - 1.

        public BoardDrawer boardDrawer;

        public Checker[] Board
        {
            get
            {
                return board;
            }
        }

        private int size;

        public int BoardSize
        {
            get
            {
                return size;
            }
        }

        public void nextPlayer()
        {
            currentPlayer = 1 - currentPlayer;
        }

        public Game(bool isP1Human, bool isP2Human, int size, int checkersRows)
        {
            this.size = size;
            board = new Checker[size*size];
            if(isP1Human)
            {
                players[0] = new HumanPlayer(this);
            }
            else
            {
                // bot here
            }
            if (isP2Human)
            {
                players[1] = new HumanPlayer(this);
            }
            else
            {
                // bot here
            }
            //inicjalizacja planszy
            for (int i=0;i< size*size;i++)
            {
                board[i] = null;
            }
            //inicjalizaje pionków
            for(int y=0;y<checkersRows;y++)
            {
                for (int x = 0; x < size / 2; x++)
                {
                    board[size * y + 2*x + (y % 2)] = new Checker(0);
                }
            }

            for (int y = size-1; y >= size-checkersRows; y--)
            {
                for (int x = 0; x < size / 2; x++)
                {
                    board[size * y + 2 * x + (y % 2)] = new Checker(1);
                }
            }
            currentPlayer = 0;
        }

        public void ProcessInputLeftButton(Point selectedField)
        {
            if(players[currentPlayer] is HumanPlayer)
            {
                (players[currentPlayer] as HumanPlayer).ProcessInputLeftButton(selectedField);
            }
        }

        public void ProcessInputRightButton(Point selectedField)
        {
            if (players[currentPlayer] is HumanPlayer)
            {
                //(players[currentPlayer] as HumanPlayer).ProcessInputRightButton(selectedField);
            }
        }

        public void MoveChecker(Point from, Point to) //ta funkcja nie waliduje już ruchu, jednie przesuwa i zbija pionki. Narazie nie działa też dla "króla".
        {
            if(Math.Abs(from.X - to.X )> 1) //taki przeskok oznacza zabicie
            {
                board[(from.Y + ((to.Y - from.Y))/2) * size + from.X + ((to.X - from.X)/2)] = null;
            }
            board[to.Y * size + to.X] = board[from.Y * size + from.X];
            board[from.Y * size + from.X] = null;
            boardDrawer.Refresh();
        }

        //zwraca liste pól z przeciwnikami dookoła. Warunki w if-ach są trochę skomplikowane, ale chyba działają poprawnie. Wbrew pozorm jest tu trochę case'ów :D
        private List<Point> enemiesAroundToKill(int px, int py)
        {
            Checker curr = board[py * size + px];
            List<Point> ret = new List<Point>();
            if(curr.isKing)
            {

            }
            else
            {
                Checker target;
                Point targetPoint = new Point(px - 1, py - 1);
                if (targetPoint.X >= 0 && targetPoint.X < size && targetPoint.Y>=0 && targetPoint.Y<size)
                {
                    target = board[(py - 1) * size + px - 1];
                    if (target != null && target.owner == 1 - curr.owner)
                    {
                        if (px - 2 >= 0 && py - 2 >=0 && board[(py - 2) * size + (px - 2)] == null)
                        {
                            ret.Add(targetPoint);
                        }
                    }
                }

                targetPoint = new Point(px + 1, py - 1);
                if (targetPoint.X >= 0 && targetPoint.X < size && targetPoint.Y >= 0 && targetPoint.Y < size)
                {
                    target = board[(py - 1) * size + px + 1];
                    if (target != null && target.owner == 1 - curr.owner)
                    {
                        if (px + 2 < size && py - 2 >=0 && board[(py - 2) * size + (px + 2)] == null)
                        {
                            ret.Add(targetPoint);
                        }
                    }
                }

                targetPoint = new Point(px + 1, py + 1);
                if (targetPoint.X >= 0 && targetPoint.X < size && targetPoint.Y >= 0 && targetPoint.Y < size)
                {
                    target = board[(py + 1) * size + px + 1];

                    if (target != null && target.owner == 1 - curr.owner)
                    {
                        if (px + 2 <size && py + 2 < size && board[(py+2) * size + (px+2)] == null)
                        {
                            ret.Add(targetPoint);
                        }
                    }
                }

                targetPoint = new Point(px - 1, py + 1);
                if (targetPoint.X >= 0 && targetPoint.X < size && targetPoint.Y >= 0 && targetPoint.Y < size)
                {
                    target = board[(py + 1) * size + px - 1];

                    if (target != null && target.owner == 1 - curr.owner)
                    {
                        if (px - 2 >= 0 && py + 2 < size && board[(py + 2) * size + (px - 2)] == null)
                        {
                            ret.Add(targetPoint);
                        }
                    }
                }

            }
            return ret;
        }

        //zwraca listę możliwych to "staniecia" pól, wokół podanego pola. Uwzględnia konieczność bicia, gdy wokół są przeciwnicy.
        public List<Point> getPossibleMoves(int px,int py)
        {
            Checker checker = board[py * size + px];

            if(checker == null)
            {
                return new List<Point>();
            }

            List<Point> ret = new List<Point>();

            List<Point> enemies = enemiesAroundToKill(px, py);

            if(enemies.Count>0)
            {
                foreach(Point en in enemies)
                {
                    ret.Add(new Point(en.X - (px - en.X), en.Y - (py - en.Y)));
                }
                return ret;
            }
            else
            {
                int tx, ty;
                if(checker.owner==0)
                {
                   tx = px + 1;
                   ty = py + 1;
                   if (tx >= 0 && tx < size && ty >= 0 && ty < size && board[ty * size + tx] == null)
                   {
                        ret.Add(new Point(tx, ty));
                   }
                   tx = px - 1;
                   if (tx >= 0 && tx < size && ty >= 0 && ty < size && board[ty * size + tx] == null)
                   {
                       ret.Add(new Point(tx, ty));
                   }
                }
                else
                {
                    tx = px + 1;
                    ty = py - 1;
                    if (tx >= 0 && tx < size && ty >= 0 && ty < size && board[ty * size + tx] == null)
                    {
                        ret.Add(new Point(tx, ty));
                    }
                    tx = px - 1;
                    if (tx >= 0 && tx < size && ty >= 0 && ty < size && board[ty * size + tx] == null)
                    {
                        ret.Add(new Point(tx, ty));
                    }
                }
            }

            return ret;
        }
    }
}