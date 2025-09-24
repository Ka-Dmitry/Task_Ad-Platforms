using AdWebService.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace AdWebService.Services
{
    public interface IMainService
    {
        ApiResponse LoadPlatforms();
        ApiResponse FindPlatformsByPath(string path);
    }

    public class MainService : IMainService
    {
        // DI
        private readonly Dictionary<string, List<string>> _data = new();
        private readonly string _path;
        public MainService(IConfiguration config)
        {
            _path = config["DataFilePath"] ?? Path.Combine(Environment.CurrentDirectory, "Data.txt");
        }
        public ApiResponse LoadPlatforms()
        {
            try
            {
                // Clear to reload
                _data.Clear();

                if (!File.Exists(_path))
                {
                    return new ApiResponse
                    {
                        ErrorMessage = "Data file not found",
                    };
                }

                // Load
                LoadDataFromFile();

                return new ApiResponse
                {
                    Response = _data,
                    IsSuccess = true
                };

            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    ErrorMessage = ex.Message,
                };
            }


        }

        private void LoadDataFromFile()
        {
            // Get all data while reading
            var platformData = new List<(string, string[] locations)>();

            using (StreamReader sr = new StreamReader(_path))
            {
                string? line = "a";
                while ((line = sr.ReadLine()) != null)
                {

                    try
                    {
                        // Process the line
                        if (line != null)
                        {
                            // Splitting to ["Platform", "Paths"]
                            string[] splittedLine;
                            splittedLine = line.Split(":");

                            string platform = splittedLine[0]; // platform
                            string[] locations = splittedLine[1].Split(",").Select(l => l.Trim()).ToArray(); // locations

                            platformData.Add((platform, locations));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Skipping the invalid line: '{line}'. Error {ex.Message}");
                        continue;
                    }
                }
            }

            // Fill Locations
            foreach (var (_, locations) in platformData)
            {
                foreach (var location in locations)
                {
                    if (!string.IsNullOrEmpty(location) && location.Length != 1)
                        _data.TryAdd(location, new List<string>());
                }
            }

            // Fill platforms inside corresponding locations
            foreach (var (platform, locations) in platformData)
            {
                foreach (var location in locations)
                {
                    if (!string.IsNullOrEmpty(location) && location.Length != 1)
                    {
                        List<string> keys = _data.Where(d => d.Key.StartsWith(location)).Select(d => d.Key).ToList();

                        foreach (var key in keys)
                        {
                            _data[key].Add(platform);
                        }
                    }
                }
            }

        }

        public ApiResponse FindPlatformsByPath(string path)
        {
            try
            {
                var decodedPath = HttpUtility.UrlDecode(path);
                
                if (!decodedPath.StartsWith("/") || string.IsNullOrEmpty(decodedPath))
                {
                    throw new Exception("Bad Path");
                }

                if (_data.TryGetValue(decodedPath, out var result))
                {
                    return new ApiResponse
                    {
                        Response = result.ToList(),
                        IsSuccess = true
                    };
                }
                
                return new ApiResponse { ErrorMessage = "Path not found"};
                

            }
            catch (Exception ex)
            {
                return new ApiResponse() { ErrorMessage = ex.Message };
            }

        }
    }
}