using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Gestiona la interfaz de usuario (HUD) que muestra información en pantalla
/// sobre el progreso y estadísticas de los agentes durante el entrenamiento.
/// </summary>
public class HUDManager : MonoBehaviour
{
    /// <summary>
    /// Referencia al script del Agente 1 para obtener sus estadísticas.
    /// </summary>
    public Agente1Script agente1;

    /// <summary>
    /// Referencia al script del Agente 2 para obtener sus estadísticas.
    /// </summary>
    public Agente2Script agente2;

    /// <summary>
    /// Texto UI para mostrar información del Agente 1.
    /// </summary>
    public TMP_Text agente1BlockText;

    /// <summary>
    /// Texto UI para mostrar información del Agente 2.
    /// </summary>
    public TMP_Text agente2BlockText;

    /// <summary>
    /// Texto UI para mostrar información general del entrenamiento.
    /// </summary>
    public TMP_Text generalBlockText;

    /// <summary>
    /// Tiempo total acumulado del entrenamiento (sumando episodios anteriores).
    /// </summary>
    private float trainingTotalTime = 0f;

    /// <summary>
    /// Tiempo de inicio del episodio actual.
    /// </summary>
    private float episodeStartTime = 0f;

    /// <summary>
    /// Inicializa el tiempo de inicio cuando comienza la escena.
    /// </summary>
    void Start()
    {
        episodeStartTime = Time.time;
    }

    /// <summary>
    /// Actualiza cada frame los textos con la información más reciente de los agentes y el entrenamiento.
    /// </summary>
    void Update()
    {
        int totalEpisodes = agente1.EpisodesTotal;

        float episodeDuration = Time.time - episodeStartTime;
        float totalTrainingTime = trainingTotalTime + episodeDuration;

        // --- Información Agente 1 ---
        float reward1 = agente1.GetCumulativeReward(); // recompensa acumulada actual
        string color1 = reward1 >= 0 ? "green" : "red"; // color verde si positiva, rojo si negativa
        float winPercent1 = totalEpisodes > 0 ? (float)agente1.EpisodesWon / totalEpisodes * 100f : 0f; // porcentaje de episodios ganados
        agente1BlockText.text =
            $"<b>Agente 1</b>\n" +
            $"Recompensa: <color={color1}>{reward1:F2}</color>\n" +
            $"Step: {agente1.StepCount}\n" +
            $"Episodios ganados: {winPercent1:F1}%";

        // --- Información Agente 2 ---
        float reward2 = agente2.GetCumulativeReward();
        string color2 = reward2 >= 0 ? "green" : "red";
        float winPercent2 = totalEpisodes > 0 ? (float)agente2.EpisodesWon / totalEpisodes * 100f : 0f;
        agente2BlockText.text =
            $"<b>Agente 2</b>\n" +
            $"Recompensa: <color={color2}>{reward2:F2}</color>\n" +
            $"Step: {agente2.StepCount}\n" +
            $"Episodios ganados: {winPercent2:F1}%";

        // --- Información general ---
        generalBlockText.text =
            $"<b>General</b>\n" +
            $"Total episodios: {totalEpisodes}\n" +
            $"Tiempo total: {FormatTime(totalTrainingTime)}\n" +
            $"Duración episodio: {FormatTime(episodeDuration)}";
    }

    /// <summary>
    /// Debe ser llamado desde OnEpisodeBegin() de cualquier agente para actualizar el tiempo total de entrenamiento.
    /// </summary>
    public void OnEpisodeBegin()
    {
        // Acumula el tiempo del episodio anterior
        trainingTotalTime += Time.time - episodeStartTime;
        // Reinicia el tiempo de inicio para el nuevo episodio
        episodeStartTime = Time.time;
    }

    /// <summary>
    /// Convierte un tiempo en segundos a formato HH:MM:SS.
    /// </summary>
    /// <param name="seconds">Tiempo en segundos</param>
    /// <returns>Cadena formateada en horas, minutos y segundos</returns>
    private string FormatTime(float seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
    }
}
