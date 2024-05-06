using System;

namespace PLinq.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Plinq_Test()
        {
            var sentense = "Mark Farragher";
            var words = sentense
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.ToLower())
                .AsParallel()
                .AsOrdered()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .Select(MoveFirstLetterToTheEnd)
                .Select(w => w + "ay")
                .ToList();

            var actual = string.Join(' ', words);

            Assert.Equal("arkmay arragherfay", actual);

        }

        [Fact]
        public void TaskNetork_Test()
        {
            var sentence = "Mark Farragher";
            var words = Map(sentence);
            var actual = Process(words).ContinueWith(t => Reduce(t.Result)).Result;

            Assert.Equal("arkmay arragherfay", actual);

        }

        private static string[] Map(string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                return new string[0];
            }
            return sentence.Split();
        }

        private static string Reduce(string[] words)
        {
            return string.Join(" ", words);
        }

        private static Task<string[]> Process(string[] words)
        {
            var tasks = new List<Task<string>>();
            for (var i = 0; i < words.Length; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() => PigLang(words[index])));
            }
            return Task.WhenAll(tasks);

        }

        private static string PigLang(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return "";
            }

            if (word.Length > 1)
            {
                word = word.Substring(1, word.Length - 1) + word[0];
            }

            return word.ToLower() + "ay";
        }

        /// <summary>
        /// anton
        /// a
        /// an
        /// </summary>
        private string MoveFirstLetterToTheEnd(string word)
        {
            if (word.Length > 1)
            {
                return word.Substring(1, word.Length - 1) + word[0];
            }

            return word;
        }
    }
}