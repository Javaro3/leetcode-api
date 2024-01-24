namespace LeetCodeApi.Models;

public class StatsModel{
    public string Username {get; set;}
    public int Rank {get; set;}
    
    public int EasyCount {get; set;}
    public int MediumCount {get; set;}
    public int HardCount {get; set;}
    public int TotalCount {get; set;}

    public int SolvedEasyCount {get; set;}
    public int SolvedMediumCount {get; set;}
    public int SolvedHardCount {get; set;}
    public int SolvedTotalCount {get; set;}

    public double EasyBeatsPercentage {get; set;}
    public double MediumBeatsPercentage {get; set;}
    public double HardBeatsPercentage {get; set;}
}