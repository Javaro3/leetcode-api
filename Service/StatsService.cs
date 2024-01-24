using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using LeetCodeApi.Models;

namespace LeetCodeApi.Service;

public class StatsService {
    private const string URL = "https://leetcode.com/graphql/";

    public StatsModel GetStats(string username){
        
        string query = $"{{\"query\":\"query getUserProfile($username: String!) {{ allQuestionsCount {{ difficulty count }} matchedUser(username: $username) {{ profile {{ ranking }} problemsSolvedBeatsStats {{ difficulty percentage }} submitStats {{ acSubmissionNum {{ difficulty count }}  }} }} }}\",\"variables\":{{\"username\":\"{username}\"}}}}";
        byte[] data = Encoding.UTF8.GetBytes(query);

        WebRequest request = WebRequest.Create(URL);
        request.Method = "POST";
        request.ContentType = "application/json";
        request.Headers.Add("referer", $"https://leetcode.com/{username}/");
        using (Stream stream = request.GetRequestStream()){
            stream.Write(data, 0, data.Length);
        }

        string responseContent;
        using (WebResponse response = request.GetResponse())
        using (Stream responseStream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(responseStream)){
            responseContent = reader.ReadToEnd();
        }
        if(responseContent.Contains("error")){
            throw new ArgumentException("Username is not exist");
        }

        var model = ConvertJsonToModel(responseContent);
        model.Username = username;
        return model;
    }

    private StatsModel ConvertJsonToModel(string json){
        JsonDocument jsonDocument = JsonDocument.Parse(json);
        JsonElement root = jsonDocument.RootElement.GetProperty("data"); 

        int rank = root
            .GetProperty("matchedUser")
            .GetProperty("profile")
            .GetProperty("ranking")
            .GetInt32();
        
        var problemArray = root.GetProperty("allQuestionsCount");
        int totalCount = problemArray[0].GetProperty("count").GetInt32();
        int easyCount = problemArray[1].GetProperty("count").GetInt32();
        int mediumCount = problemArray[2].GetProperty("count").GetInt32();
        int hardCount = problemArray[3].GetProperty("count").GetInt32();
        
        var problemSolvedArray = root
            .GetProperty("matchedUser")
            .GetProperty("submitStats")
            .GetProperty("acSubmissionNum");
        int totalSolvedCount = problemSolvedArray[0].GetProperty("count").GetInt32();
        int easySolvedCount = problemSolvedArray[1].GetProperty("count").GetInt32();
        int mediumSolvedCount = problemSolvedArray[2].GetProperty("count").GetInt32();
        int hardSolvedCount = problemSolvedArray[3].GetProperty("count").GetInt32();

        var beatsArray = root
            .GetProperty("matchedUser")
            .GetProperty("problemsSolvedBeatsStats");
        double easyBeatsPercentage = beatsArray[0].GetProperty("percentage").GetDouble();
        double mediumBeatsPercentage = beatsArray[1].GetProperty("percentage").GetDouble();
        double hardBeatsPercentage = beatsArray[2].GetProperty("percentage").GetDouble();

        StatsModel model = new StatsModel(){
            Rank = rank,
            TotalCount = totalCount,
            EasyCount = easyCount,
            MediumCount = mediumCount,
            HardCount = hardCount,
            SolvedTotalCount = totalSolvedCount,
            SolvedEasyCount = easySolvedCount,
            SolvedMediumCount = mediumSolvedCount,
            SolvedHardCount = hardSolvedCount,
            EasyBeatsPercentage = easyBeatsPercentage,
            MediumBeatsPercentage = mediumBeatsPercentage,
            HardBeatsPercentage = hardBeatsPercentage
        };
        
        return model;
    }
}