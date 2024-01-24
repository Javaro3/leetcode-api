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

    public string ConvertModelToSvg(StatsModel model){
        double circleRadius = 377 * (model.SolvedTotalCount*1f / model.TotalCount);
        int easyLength = (int)(200 + (200 * (model.SolvedEasyCount*1.0 / model.EasyCount)));
        int mediumLength = (int)(200 + (200 * (model.SolvedMediumCount*1.0 / model.MediumCount)));
        int hardLength = (int)(200 + (200 * (model.SolvedHardCount*1.0 / model.HardCount)));
        return 
            @$"<svg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' 
            style='isolation: isolate' viewBox='0 0 410 186' width='410px' height='186px'>
                <rect width='410px' height='186px' rx='10' ry='10' fill='#FFFFFF' style='stroke:#F7F8FA;stroke-width:3;opacity:1' />  
                
                <text x='16' y='25' font-family='Helvetica' font-size='10px' fill='#E0DDD8' font-weight='bold'>{model.Username} (Rank: {model.Rank})</text>
                
                <circle r='60' cx='90' cy='105' fill='none' stroke='#E0DDD8' stroke-width='3px'></circle>
                <circle r='60' cx='90' cy='105' fill='none' stroke='#FFA116' stroke-width='5px' stroke-linecap='round' stroke-dasharray='{circleRadius}, 377' transform='rotate(-90 90 105)' ></circle>
                <text x='90' y='105' font-family='Helvetica' text-anchor='middle' font-size='30px' fill='#26262D' font-weight='bold' >{model.SolvedTotalCount}</text>
                <text x='90' y='125' font-family='Helvetica' text-anchor='middle' font-size='15px' fill='#E0DDD8' font-weight='bold' >Solved</text>
                
                <text x='200' y='60' font-family='Helvetica' text-anchor='start' font-size='10px' fill='#E0DDD8'>Easy</text>
                <text x='250' y='60' font-family='Helvetica' text-anchor='start' font-size='10px' fill='#26262D' font-weight='bold'>
                    {model.SolvedEasyCount} <tspan fill='#E0DDD8' font-size='8px'>/{model.EasyCount}</tspan>
                </text>
                <text x='350' y='60' font-family='Helvetica' text-anchor='start' font-size='8px' fill='#E0DDD8' font-weight='bold'>Beats {model.EasyBeatsPercentage}%</text>
                <line x1='200' y1='70' x2='400' y2='70' stroke='#E0F4E7' stroke-width='7' stroke-linecap='round' />
                <line x1='200' y1='70' x2='{easyLength}' y2='70' stroke='#00AF9B' stroke-width='7' stroke-linecap='round' />

                <text x='200' y='100' font-family='Helvetica' text-anchor='start' font-size='10px' fill='#E0DDD8'>Medium</text>
                <text x='250' y='100' font-family='Helvetica' text-anchor='start' font-size='10px' fill='#26262D' font-weight='bold'>
                    {model.SolvedMediumCount} <tspan fill='#E0DDD8' font-size='8px'>/{model.MediumCount}</tspan>
                </text>
                <text x='350' y='100' font-family='Helvetica' text-anchor='start' font-size='8px' fill='#E0DDD8' font-weight='bold'>Beats {model.MediumBeatsPercentage}%</text>
                <line x1='200' y1='110' x2='400' y2='110' stroke='#FFF4D9' stroke-width='7' stroke-linecap='round' />
                <line x1='200' y1='110' x2='{mediumLength}' y2='110' stroke='#FFB800' stroke-width='7' stroke-linecap='round' />

                <text x='200' y='140' font-family='Helvetica' text-anchor='start' font-size='10px' fill='#E0DDD8'>Hard</text>
                <text x='250' y='140' font-family='Helvetica' text-anchor='start' font-size='10px' fill='#26262D' font-weight='bold'>
                    {model.SolvedHardCount} <tspan fill='#E0DDD8' font-size='8px' >/{model.HardCount}</tspan>
                </text>
                <text x='350' y='140' font-family='Helvetica' text-anchor='start' font-size='8px' fill='#E0DDD8' font-weight='bold'>Beats {model.HardBeatsPercentage}%</text>
                <line x1='200' y1='150' x2='400' y2='150' stroke='#FDE4E3' stroke-width='7' stroke-linecap='round' />
                <line x1='200' y1='150' x2='{hardLength}' y2='150' stroke='#EF4743' stroke-width='7' stroke-linecap='round' />
            </svg>";
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