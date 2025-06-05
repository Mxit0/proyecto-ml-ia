using UnityEngine;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// Gestiona la interfaz de usuario (HUD) que muestra información agregada
/// sobre el progreso y estadísticas de los agentes durante el entrenamiento
/// en todos los ambientes activos.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Referencias a todos los agentes en la escena")]
    public Agente1Script[] agentes1;
    public Agente2Script[] agentes2;

    [Header("UI")]
    public TMP_Text agente1BlockText;
    public TMP_Text agente2BlockText;
    public TMP_Text generalBlockText;

    private float trainingTotalTime = 0f;
    private float episodeStartTime = 0f;

    void Start()
    {
        episodeStartTime = Time.time;
        // Si no se asignan manualmente, buscar automáticamente todos los agentes en la escena
        if (agentes1 == null || agentes1.Length == 0)
            agentes1 = UnityEngine.Object.FindObjectsByType<Agente1Script>(FindObjectsSortMode.None);
        if (agentes2 == null || agentes2.Length == 0)
            agentes2 = UnityEngine.Object.FindObjectsByType<Agente2Script>(FindObjectsSortMode.None);
    }

    void Update()
    {
        // --- Agente 1 ---
        int totalEpisodes1 = agentes1.Sum(a => a.EpisodesTotal);
        int totalWins1 = agentes1.Sum(a => a.EpisodesWon);
        float avgReward1 = agentes1.Length > 0 ? agentes1.Average(a => a.GetCumulativeReward()) : 0f;

        // --- Agente 2 ---
        int totalEpisodes2 = agentes2.Sum(a => a.EpisodesTotal);
        int totalWins2 = agentes2.Sum(a => a.EpisodesWon);
        float avgReward2 = agentes2.Length > 0 ? agentes2.Average(a => a.GetCumulativeReward()) : 0f;

        // --- Porcentaje de victorias sobre la suma de victorias ---
        int totalVictorias = totalWins1 + totalWins2;
        totalVictorias = totalVictorias == 0 ? 1 : totalVictorias; // evitar división por cero

        float winPercent1 = (float)totalWins1 / totalVictorias * 100f;
        float winPercent2 = (float)totalWins2 / totalVictorias * 100f;

        agente1BlockText.text =
            $"<b>Agente 1 (Todos)</b>\n" +
            $"Recompensa promedio: <color={(avgReward1 >= 0 ? "green" : "red")}>{avgReward1:F2}</color>\n" +
            $"Episodios ganados: {winPercent1:F1}%\n" +
            $"Total episodios: {totalEpisodes1}";

        agente2BlockText.text =
            $"<b>Agente 2 (Todos)</b>\n" +
            $"Recompensa promedio: <color={(avgReward2 >= 0 ? "green" : "red")}>{avgReward2:F2}</color>\n" +
            $"Episodios ganados: {winPercent2:F1}%\n" +
            $"Total episodios: {totalEpisodes2}";

        // --- General ---
        float episodeDuration = Time.time - episodeStartTime;
        float totalTrainingTime = trainingTotalTime + episodeDuration;

        generalBlockText.text =
            $"<b>General</b>\n" +
            $"Ambientes activos: {Mathf.Max(agentes1.Length, agentes2.Length)}\n" +
            $"Tiempo total: {FormatTime(totalTrainingTime)}\n";
    }

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
