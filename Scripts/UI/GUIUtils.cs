namespace NS.UI
{
    public class GUIUtils
    {
        public static void HandleSaveAndExit(GUIDialog dialog)
        {
            if (Game.Instance.RequiresSave())
            {
                dialog.Show("Are you sure you want to quit to desktop?",
                    yesText: "Save and Quit",
                    onYes: Game.Instance.MaybeSaveAndQuit,
                    noText: "Just quit",
                    onNo: Game.Instance.Quit,
                    cancelText: "Don't Quit",
                    showInputField: true
                );
            }
            else
            {
                dialog.Show("Are you sure you want to quit to desktop?\n" +
                            "Your game save is up to date.",
                    onYes: _ => Game.Instance.Quit()
                );
            }
        }

        public static void HandleStartNewGame(GUIDialog dialog)
        {
            if (Game.Instance.RequiresSave())
            {
                dialog.Show("Do you want to save your current game?",
                    yesText: "Save and start new",
                    onYes: saveName =>
                    {
                        Game.Instance.SaveCurrentGame(saveName);
                        Game.Instance.SetupAndBeginNewGame();
                    },
                    noText: "Don't save and start new",
                    onNo: Game.Instance.SetupAndBeginNewGame,
                    cancelText: "Cancel",
                    showInputField: true
                );
            }
            else
            {
                Game.Instance.SetupAndBeginNewGame();
            }
        }
    }
}
