using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace Insight_test.All.Services
{
    /// <summary>
    /// Default implementation of report text chunking.
    /// </summary>
    public class ChunkingService : IChunkingService
    {
        // Target chunk size range.
        private const int MinChunkSize = 300;
        private const int MaxChunkSize = 500;

        public List<ChunkDto> SplitIntoChunks(string fullText)
        {
            var chunks = new List<ChunkDto>();

            if (string.IsNullOrWhiteSpace(fullText))
                return chunks;

            var sentences = SplitIntoSentences(fullText);

            var currentChunk = new StringBuilder();
            int chunkIndex = 0;

            foreach (var sentence in sentences)
            {
                // If the chunk already has enough content, and adding this
                // sentence would make it too big, close the chunk first.
                bool chunkIsBigEnough = currentChunk.Length >= MinChunkSize;
                bool wouldOverflow = currentChunk.Length + sentence.Length > MaxChunkSize;

                if (currentChunk.Length > 0 && chunkIsBigEnough && wouldOverflow)
                {
                    chunks.Add(new ChunkDto
                    {
                        Text = currentChunk.ToString().Trim(),
                        ChunkIndex = chunkIndex++
                    });
                    currentChunk.Clear();
                }

                currentChunk.Append(sentence).Append(' ');
            }

            // Whatever is left over becomes the last chunk.
            if (currentChunk.Length > 0)
            {
                chunks.Add(new ChunkDto
                {
                    Text = currentChunk.ToString().Trim(),
                    ChunkIndex = chunkIndex
                });
            }

            return chunks;
        }

        // Splits text into sentences using punctuation boundaries.
        private List<string> SplitIntoSentences(string text)
        {
            var sentences = new List<string>();

            // Split after sentence-ending punctuation.
            var pattern = @"(?<=[\.\!\?])\s+";
            var rawParts = Regex.Split(text.Trim(), pattern);

            foreach (var part in rawParts)
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    sentences.Add(trimmed);
            }

            return sentences;
        }
    }
}
