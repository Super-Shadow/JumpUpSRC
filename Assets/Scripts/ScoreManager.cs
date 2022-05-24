using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{

    // Maybe cool in the future to have points gradually added (like lerping
    // Would basically just add points slowly, fast then slowly again, 1 by 1
    // rather then instant point gain

    private static int _score;
    
    public int Score { get => _score; }
    
    public void AddPoints(int points)
    {
        _score += points;
    }

    // public void AddPoints(int points) {}

    public void RemovePoints(int points)
    {
        _score -= points;
        if (_score < 0)
            _score = 0;
    }
}
