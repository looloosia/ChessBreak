using UnityEngine;

public class Constants
{
    public const int BOARD_SIZE = 8;
    public enum GameResult
    {
        None, Win, Lose, Draw
    }

    public enum PlayerType
    {
        None, Human, AI
    }

    public enum PlayerColor
    {
        None, White, Black
    }

    public enum PieceType
    {
        None, Pawn, Rook, Knight, Bishop, Queen, King
    }

    public enum GameType
    {
        SinglePlay, LocalDualPlay, MultiPlay
    }
}
