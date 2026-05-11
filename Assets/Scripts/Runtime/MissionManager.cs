using UnityEngine;

namespace SnowSurfer
{
    public sealed class MissionManager : MonoBehaviour
    {
        [SerializeField] private int missionBonus = 120;

        private ScoreManager scoreManager;
        private bool giftMissionComplete;
        private bool survivalMissionComplete;
        private bool scoreMissionComplete;
        private string currentMessage = "Collect 10 gifts";

        public string CurrentMessage => currentMessage;

        public void Initialize(ScoreManager score)
        {
            scoreManager = score;
        }

        public void ResetRun()
        {
            giftMissionComplete = false;
            survivalMissionComplete = false;
            scoreMissionComplete = false;
            currentMessage = "Collect 10 gifts";
        }

        public void Tick()
        {
            if (scoreManager == null)
            {
                return;
            }

            if (!giftMissionComplete && scoreManager.GiftsCollected >= 10)
            {
                giftMissionComplete = true;
                Complete("Gift streak complete");
            }
            else if (!survivalMissionComplete && scoreManager.SurvivedTime >= 30f)
            {
                survivalMissionComplete = true;
                Complete("30 second run complete");
            }
            else if (!scoreMissionComplete && scoreManager.Score >= 500)
            {
                scoreMissionComplete = true;
                Complete("500 score complete");
            }
            else
            {
                UpdateCurrentMessage();
            }
        }

        private void Complete(string message)
        {
            scoreManager.AddMissionBonus(missionBonus);
            currentMessage = message + " +" + missionBonus;
            FloatingText.Spawn(new Vector3(0f, 1.6f, 0f), currentMessage);
        }

        private void UpdateCurrentMessage()
        {
            if (!giftMissionComplete)
            {
                currentMessage = "Collect gifts " + scoreManager.GiftsCollected + "/10";
            }
            else if (!survivalMissionComplete)
            {
                currentMessage = "Survive " + Mathf.FloorToInt(scoreManager.SurvivedTime) + "/30s";
            }
            else if (!scoreMissionComplete)
            {
                currentMessage = "Reach score " + scoreManager.Score + "/500";
            }
            else
            {
                currentMessage = "All goals complete";
            }
        }
    }
}
