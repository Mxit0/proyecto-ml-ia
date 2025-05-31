using UnityEngine;
using TMPro;
using System;

public class HUDManager : MonoBehaviour
{
    public Agente1Script agente1;
    public Agente2Script agente2;

    public TMP_Text agente1BlockText;
    public TMP_Text agente2BlockText;
    public TMP_Text generalBlockText;

    private float trainingTotalTime = 0f;
    private float episodeStartTime = 0f;

    void Start()
    {
        episodeStartTime = Time.time;
    }

    void Update()
    {
        // General
        int totalEpisodes = (agente1.EpisodesTotal + agente2.EpisodesTotal) / 2;
        float episodeDuration = Time.time - episodeStartTime;
        float totalTrainingTime = (trainingTotalTime + episodeDuration) / 2f;

        // Agente 1
        float reward1 = agente1.GetCumulativeReward();
        string color1 = reward1 >= 0 ? "green" : "red";
        float winPercent1 = totalEpisodes > 0 ? (float)agente1.EpisodesWon / totalEpisodes * 100f : 0f;
        agente1BlockText.text =
            $"<b>Agente 1</b>\n" +
            $"Recompensa: <color={color1}>{reward1:F2}</color>\n" +
            $"Step: {agente1.StepCount}\n" +
            $"Episodios ganados: {winPercent1:F1}%";

        // Agente 2
        float reward2 = agente2.GetCumulativeReward();
        string color2 = reward2 >= 0 ? "green" : "red";
        float winPercent2 = totalEpisodes > 0 ? (float)agente2.EpisodesWon / totalEpisodes * 100f : 0f;
        agente2BlockText.text =
            $"<b>Agente 2</b>\n" +
            $"Recompensa: <color={color2}>{reward2:F2}</color>\n" +
            $"Step: {agente2.StepCount}\n" +
            $"Episodios ganados: {winPercent2:F1}%";

        generalBlockText.text =
            $"<b>General</b>\n" +
            $"Total episodios: {totalEpisodes}\n" +
            $"Tiempo total: {FormatTime(totalTrainingTime)}\n" +
            $"Duración episodio: {FormatTime(episodeDuration)}";
    }

    // Llama a este método desde OnEpisodeBegin() de cualquier agente
    public void OnEpisodeBegin()
    {
        trainingTotalTime += Time.time - episodeStartTime;
        episodeStartTime = Time.time;
    }

    private string FormatTime(float seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
    }
}
