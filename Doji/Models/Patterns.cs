// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;

namespace Doji
{
    public static class Patterns
    {
        private const string _recentPatternsStorageKey = "uct-recent-patterns";

        private static List<PatternCategory> _patternsCategories;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private static LinkedList<Pattern> _recentPatterns;
        private static RoamingObjectStorageHelper _roamingObjectStorageHelper = new RoamingObjectStorageHelper();

        public static async Task<PatternCategory> GetCategoryByPattern(Pattern pattern)
        {
            return (await GetCategoriesAsync()).FirstOrDefault(c => c.Patterns.Contains(pattern));
        }

        public static async Task<PatternCategory> GetCategoryByName(string name)
        {
            return (await GetCategoriesAsync()).FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<Pattern> GetPatternByName(string name)
        {
            return (await GetCategoriesAsync()).SelectMany(c => c.Patterns).FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<Pattern[]> FindPatternsByName(string name)
        {
            var query = name.ToLower();
            return (await GetCategoriesAsync()).SelectMany(c => c.Patterns).Where(s => s.Name.ToLower().Contains(query)).ToArray();
        }

        public static async Task<List<PatternCategory>> GetCategoriesAsync()
        {
            await _semaphore.WaitAsync();
            if (_patternsCategories == null)
            {
                List<PatternCategory> allCategories;
                using (var jsonStream = await StreamHelper.GetPackagedFileStreamAsync("PatternPages/patterns.json"))
                {
                    var jsonString = await jsonStream.ReadTextAsync();
                    allCategories = JsonConvert.DeserializeObject<List<PatternCategory>>(jsonString);
                }

                // Check API
                var supportedCategories = new List<PatternCategory>();
                foreach (var category in allCategories)
                {
                    var finalPatterns = new List<Pattern>();

                    foreach (var pattern in category.Patterns)
                    {
                        if (pattern.IsSupported)
                        {
                            finalPatterns.Add(pattern);
                            await pattern.PreparePropertyDescriptorAsync();
                        }
                    }

                    if (finalPatterns.Count > 0)
                    {
                        supportedCategories.Add(category);
                        category.Patterns = finalPatterns.OrderBy(s => s.Name).ToArray();
                    }
                }

                _patternsCategories = supportedCategories.ToList();
            }

            _semaphore.Release();
            return _patternsCategories;
        }

        public static async Task<LinkedList<Pattern>> GetRecentPatterns()
        {
            if (_recentPatterns == null)
            {
                _recentPatterns = new LinkedList<Pattern>();
                var savedPatterns = _roamingObjectStorageHelper.Read<string>(_recentPatternsStorageKey);

                if (savedPatterns != null)
                {
                    var patternNames = savedPatterns.Split(';').Reverse();
                    foreach (var name in patternNames)
                    {
                        var pattern = await GetPatternByName(name);
                        if (pattern != null)
                        {
                            _recentPatterns.AddFirst(pattern);
                        }
                    }
                }
            }

            return _recentPatterns;
        }

        public static async Task PushRecentPattern(Pattern pattern)
        {
            var patterns = await GetRecentPatterns();

            var duplicates = patterns.Where(s => s.Name == pattern.Name).ToList();
            foreach (var duplicate in duplicates)
            {
                patterns.Remove(duplicate);
            }

            patterns.AddFirst(pattern);
            while (patterns.Count > 10)
            {
                patterns.RemoveLast();
            }

            SaveRecentPatterns();
        }

        private static void SaveRecentPatterns()
        {
            if (_recentPatterns == null)
            {
                return;
            }

            var str = string.Join(";", _recentPatterns.Take(10).Select(s => s.Name).ToArray());
            _roamingObjectStorageHelper.Save<string>(_recentPatternsStorageKey, str);
        }
    }
}
