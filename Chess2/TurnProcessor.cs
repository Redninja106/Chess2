using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2;
internal class TurnProcessor
{
    private Dictionary<Team, List<Move>> moveLists = [];

    public void AddMoveForTeam(Team team, Move move)
    {
        if (!moveLists.TryGetValue(team, out var list))
        {
            moveLists[team] = list = [];
        }
        list.Add(move);
    }

    public void FinishTurn()
    {
        foreach (var (team, moveList) in moveLists)
        {
            foreach (var move in moveList)
            {
                move.Execute();
            }
        }
        moveLists.Clear();
    }
}
