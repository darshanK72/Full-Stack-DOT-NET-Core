/*
 * PROBLEM: Warehouse Pick Queue
 *
 * FIFO pick tickets, LIFO unit undo, and BFS grid navigation.
 *
 * This exercise covers:
 *   ch06 — Queue<T> FIFO, Stack<T> LIFO, TryDequeue/TryPop
 *   ch06 — BFS with Queue frontier
 */

using System;
using System.Collections.Generic;

namespace WarehousePicking
{
    readonly record struct PickTicket(int Id, string Sku, int Units);

    class PickFloorSession
    {
        private readonly Queue<PickTicket> _pending = new Queue<PickTicket>();
        private readonly Stack<int> _unitsUndoStack = new Stack<int>();
        private PickTicket? _activeTicket = null;

        public int PendingCount => _pending.Count;
        public int? ActiveTicketId => _activeTicket?.Id;

        public void EnqueueTicket(PickTicket ticket) => _pending.Enqueue(ticket);

        public bool TryStartNextPick(out PickTicket ticket)
        {
            // TODO: TryDequeue, clear undo stack, set active ticket
            throw new NotImplementedException();
        }

        public void AdjustUnits(int newUnits)
        {
            // TODO: Push current units, update active ticket
            throw new NotImplementedException();
        }

        public bool TryUndoUnits(out int restoredUnits)
        {
            // TODO: TryPop undo stack, restore active ticket units
            throw new NotImplementedException();
        }
    }

    static class GridNavigator
    {
        public static bool TryFindGoal(char[,] grid, int startRow, int startCol, out (int Row, int Col) goalCell)
        {
            // TODO: BFS with Queue<(int,int)>; '.' open, '#' wall, 'G' goal
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: enqueue 3 tickets, adjust + undo demo, drain queue
            // TODO: 4x4 maze BFS sample from reading chapter
            throw new NotImplementedException();
        }
    }
}
