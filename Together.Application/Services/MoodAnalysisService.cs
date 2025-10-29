using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class MoodAnalysisService : IMoodAnalysisService
{
    private readonly IMoodEntryRepository _moodEntryRepository;

    public MoodAnalysisService(IMoodEntryRepository moodEntryRepository)
    {
        _moodEntryRepository = moodEntryRepository;
    }

    public async Task<MoodTrendDto> AnalyzeMoodTrendAsync(Guid userId, int days = 30)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);
        var toDate = DateTime.UtcNow;

        var moodEntries = await _moodEntryRepository.GetUserMoodsAsync(userId, fromDate, toDate);
        var moodList = moodEntries.ToList();

        if (!moodList.Any())
        {
            return new MoodTrendDto(
                "Neutral",
                0,
                new Dictionary<string, int>(),
                new List<MoodEntryDto>()
            );
        }

        // Calculate mood scores (positive moods = higher scores)
        var moodScores = new Dictionary<MoodType, double>
        {
            { MoodType.Happy, 5 },
            { MoodType.Excited, 4 },
            { MoodType.Calm, 3 },
            { MoodType.Stressed, 2 },
            { MoodType.Anxious, 1 },
            { MoodType.Sad, 0 },
            { MoodType.Angry, 0 }
        };

        var averageScore = moodList.Average(m => moodScores[m.Mood]);

        // Determine trend type
        string trendType;
        if (averageScore >= 4)
            trendType = "Very Positive";
        else if (averageScore >= 3)
            trendType = "Positive";
        else if (averageScore >= 2)
            trendType = "Neutral";
        else
            trendType = "Negative";

        // Calculate mood distribution
        var moodDistribution = moodList
            .GroupBy(m => m.Mood.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Get recent entries (last 10)
        var recentEntries = moodList
            .Take(10)
            .Select(m => new MoodEntryDto(
                m.Id,
                m.UserId,
                m.Mood.ToString(),
                m.Notes,
                m.Timestamp
            ))
            .ToList();

        return new MoodTrendDto(
            trendType,
            averageScore,
            moodDistribution,
            recentEntries
        );
    }

    public Task<string> GenerateSupportMessageAsync(MoodType mood)
    {
        var supportMessages = new Dictionary<MoodType, List<string>>
        {
            {
                MoodType.Sad, new List<string>
                {
                    "I'm here for you. Would you like to talk about what's on your mind?",
                    "It's okay to feel sad sometimes. I'm thinking of you.",
                    "Sending you a virtual hug. Remember, this feeling will pass.",
                    "You're not alone. I care about you and I'm here to listen."
                }
            },
            {
                MoodType.Anxious, new List<string>
                {
                    "Take a deep breath. I'm here with you through this.",
                    "Remember, you've gotten through tough times before. You've got this.",
                    "Let's take things one step at a time. What can I do to help?",
                    "Your feelings are valid. Want to talk about what's making you anxious?"
                }
            },
            {
                MoodType.Angry, new List<string>
                {
                    "I can see you're upset. I'm here to listen when you're ready to talk.",
                    "It's okay to feel angry. Let's work through this together.",
                    "Take some time if you need it. I'll be here when you want to share.",
                    "Your feelings matter to me. What happened?"
                }
            },
            {
                MoodType.Stressed, new List<string>
                {
                    "You're doing great, even if it doesn't feel like it right now.",
                    "Let's tackle this together. What's the most pressing thing on your mind?",
                    "Remember to take breaks. You deserve rest and care.",
                    "I believe in you. You're stronger than you think."
                }
            }
        };

        if (supportMessages.ContainsKey(mood))
        {
            var messages = supportMessages[mood];
            var random = new Random();
            var selectedMessage = messages[random.Next(messages.Count)];
            return Task.FromResult(selectedMessage);
        }

        return Task.FromResult("I'm here for you. How are you feeling?");
    }
}
