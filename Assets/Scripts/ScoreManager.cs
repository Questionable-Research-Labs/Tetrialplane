using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Scripts {
    public class ScoreManager : MonoBehaviour {
        /// <summary>
        /// The current score
        /// </summary>
        public TMP_Text scoreText;

        /// <summary>
        /// How many points should be deducted for missing a block 
        /// </summary>
        public int missedPieceScoreDeduction = 100;

        /// <summary>
        /// How many point should bee added when a player clears a plane
        /// </summary>
        public int planeClearScoreAddition = 300;

        /// <summary>
        /// How much the number of points should be multiplied by when scoring points
        /// </summary>
        public float planeClearScoreMultiplier = 1.5f;

        /// <summary>
        /// The number of points for clearing a tetris
        /// </summary>
        public int tetrisPlaneClearScoreAddition = 2000;

        private int _score = 0;
        public int scoreIncrement = 50;

        /**
         * <summary>
         *  When the player misses a piece
         * </summary>
         * <param name="count">The number of pieces missed</param>
         */
        public void PieceMissed(int count) {
            UpdateScore(-missedPieceScoreDeduction * count);
        }

        /** <summary>
         * When the player clears a plane
         * </summary>
         * <param name="numberCleared">The number of planes cleared</param>
         */
        public void PlanesCleared(int numberCleared) {
            switch (numberCleared) {
                case 1:
                    UpdateScore(planeClearScoreAddition);
                    break;
                case 4:
                    UpdateScore(tetrisPlaneClearScoreAddition);
                    break;
                default:
                    UpdateScore(Mathf.FloorToInt(tetrisPlaneClearScoreAddition * numberCleared *
                                                 planeClearScoreMultiplier));
                    break;
            }
        }

        public void IncreaseScore() {
            _score += scoreIncrement;
            Debug.Log(_score);
        }

        public void ResetScore() {
            _score = 0;
        }

        /**
         * <summary>
         * Sends a PUT request to the server updating the score
         * for the provided name
         * </summary>
         * <param name="playerName">The name of the player to update</param>
         * <param name="score">The total score that player has</param>
         */
        public static IEnumerator UpdateScoreServer(string playerName, int score) {
            var form = new WWWForm();
            form.AddField("name", playerName);
            form.AddField("score", score);
            var request = UnityWebRequest.Post("https://tp.host.qrl.nz/api/board",form);
            yield return request.SendWebRequest();
        }

        /** <summary>
         * Update and redraw the score
         * </summary>
         * <param name="score">The number to be added to the score</param>
         */
        private void UpdateScore(int score) {
            _score += score;
            if (scoreText != null)
                scoreText.text = "" + _score;
        }
    }
}