﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BaboonAPI.Hooks.Tracks;
using TrombLoader.CustomTracks;
using TrombLoader.Helpers;
using UnityEngine;

namespace HighscoreAccuracy;

public static class Utils
{
    public static string FormatDecimals<T>(this T _number) where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        return string.Format(new NumberFormatInfo() { NumberDecimalDigits = (int)Plugin.decimals.Value }, "{0:F}", _number);
    }

    public static string ScoreLetter(float num) =>
        num < 1f ? (num < 0.8f ? (num < 0.6f ? (num < 0.4f ? (num < 0.2f ? "F" : "D") : "C") : "B") : "A") : "S";

    public static int GetMaxScore(AccType accType, List<float[]> levelData) => GetScoreSums(accType, levelData)[levelData.Count - 1];

    public static int[] GetScoreSums(AccType accType, List<float[]> levelData)
    {
        int[] scoreSums = new int[levelData.Count];
        int scoreTotal = 0;
        int index = 0;
        float minimumNoteGap = .025f;
        for (int i = 0; i < levelData.Count; i++)
        {
            var length = levelData[i][1];
            while (i + 1 < levelData.Count && levelData[i][0] + levelData[i][1] + minimumNoteGap >= levelData[i + 1][0])
            {
                length += levelData[i + 1][1];
                scoreSums[i] = scoreTotal;
                i++;
            }
            var score = accType switch
            {
                AccType.BaseGame => GetGameMax(length),
                _ => GetRealMax(length, index),
            };
            scoreTotal += score;
            scoreSums[i] = scoreTotal;
            index++;
        }
        return scoreSums;
    }

    private static int GetRealMax(float length, int noteIndex)
    {
        double champbonus = noteIndex > 23 ? 1.5 : 0;
        double realCoefficient = (Math.Min(noteIndex, 10) + champbonus) * 0.100000001490116 + 1.0;
        length = GetBaseScore(length);
        return (int)(Mathf.Floor((float)((double)length * 100 * realCoefficient)) * 10f);
    }

    private static int GetGameMax(float length) => (int)Mathf.Floor(Mathf.Floor(GetBaseScore(length) * 100f * 1.315f) * 10f);

    private static float GetBaseScore(float length) => Mathf.Clamp(length, 0.2f, 5f) * 8f + 10f;

    public static List<float[]> GetLevelData(string trackRef) =>
        TrackLookup.lookup(trackRef).LoadChart().savedleveldata;

    public static bool SkipHighscore(string trackRef)
    {
        return TrackLookup.lookup(trackRef) is CustomTrack ct
            && new FileInfo(Path.Combine(ct.folderPath, Globals.defaultChartName)).Length > 2_000_000;
    }
}
