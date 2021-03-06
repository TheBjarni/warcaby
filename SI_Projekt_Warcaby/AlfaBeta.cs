﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Warcaby
{
    class AlfaBeta : MinMax
    {

        /// <summary>
        /// Wymagany konstruktor
        /// </summary>
        public AlfaBeta(Game g, Form2 displayer)
            : base(g, displayer)
        {

        }

        protected override void runAlgorithm()
        {
            root.Alfa = Int16.MinValue;
            root.Beta = Int16.MaxValue;
            base.runAlgorithm();
            MessageBox.Show("AlfaBeta, liczba liści: " + numOfCheckedLeafs);
        }
        protected override void max(Node node)
        {
            if (node == null)
                throw new NullReferenceException("Niezainicjalizowany wierzcholek.");
            node.isLost = true;
            if (node.Children == null)
            {
                if (node.CurrentPlayer == game.CurrentPlayer)
                    node.Value = ratePositions(node.Board, node.CurrentPlayer);
                else node.Value = 0;
                
            }
            else
            {
                for (int i = 0; i < node.Children.Length; i++)
                {
                    if (node.Children[i].Value == -1 && node.Alfa < node.Beta)
                        min(node.Children[i]);
                    else if (node.Alfa >= node.Beta)
                        node.Children[i].Disabled = true;
                }
                for (int i = 0; i < node.Children.Length; i++)
                    if (node.Children[i].Value > node.Value && !node.Children[i].Disabled)
                    {
                        node.Value = node.Children[i].Value;
                        node.ChosenOne = node.Children[i];
                    }
            }
            Node parent = node.Parent;
            if (parent != null && node.Value < parent.Beta)
            {
                parent.Beta = node.Value;
                for (int i = 0; i < parent.Children.Length; i++)
                {
                    if (parent.Children[i] != node)
                        parent.Children[i].Beta = node.Value;
                }
            }
            node.Board = null; //ugly memory optimization 

        }

        protected override void min(Node node)
        {
            if (node == null)
                throw new NullReferenceException("Niezainicjalizowany wierzcholek.");
            node.isLost = true;
            if (node.Children == null)
            {
                if (node.CurrentPlayer == game.CurrentPlayer)
                    node.Value = ratePositions(node.Board, node.CurrentPlayer);
                else node.Value = 0;
            }
            else
            {
                node.Value = Int16.MaxValue;
                for (int i = 0; i < node.Children.Length; i++)
                {
                    if (node.Children[i].Value == -1 && node.Alfa < node.Beta)
                        max(node.Children[i]);
                    else if (node.Alfa >= node.Beta)
                        node.Children[i].Disabled = true;
                }
                for (int i = 0; i < node.Children.Length; i++)
                    if (node.Children[i].Value < node.Value && !node.Children[i].Disabled)
                    {
                        node.Value = node.Children[i].Value;
                        node.ChosenOne = node.Children[i];
                    }
            }

            Node parent = node.Parent;
            if (parent != null && node.Value > parent.Alfa)
            {
                parent.Alfa = node.Value;
                for (int i = 0; i < parent.Children.Length; i++)
                {
                    if (parent.Children[i] != node)
                        parent.Children[i].Alfa = node.Value;
                }
            }
            node.Board = null; //ugly memory optimization   

        }

        protected override void buildTree(Node node, int levelsLeft, Game.Checker[] board)
        {
            if (node == null)
                throw new NullReferenceException("Niezainicjalizowany wierzcholek.");
            var moves = Game.getPossibleMoves(node.CurrentPlayer, board, game.BoardSize);
            node.Children = new Node[moves.Count];
            //node.Moves = moves; //We don't need to store available moves for every node, just for the root
            for (int i = 0; i < moves.Count; i++)
            {
                node.Children[i] = new Node();
                node.Children[i].Parent = node;
                node.Children[i].Value = -1;        //Nieobliczona wartość
                node.Children[i].Alfa = Int16.MinValue;
                node.Children[i].Beta = Int16.MaxValue;
                node.Children[i].Disabled = false;
                node.Children[i].CurrentPlayer = node.CurrentPlayer == 0 ? 1 : 0;
                var newBoard = Game.GetBoardCopy(board);
                Game.MoveChecker(moves[i], newBoard, game.BoardSize);
                node.Children[i].Board = newBoard;
                if (levelsLeft > 1)
                    //node.Children[i].Value = ratePositions();
                    buildTree(node.Children[i], levelsLeft - 1, newBoard);
            }
        }
    }
}
