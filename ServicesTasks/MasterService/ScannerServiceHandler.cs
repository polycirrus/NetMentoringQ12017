using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace MasterService
{
    public class ScannerServiceHandler
    {
        private static readonly string IdleStateName = "Idle";
        private static readonly string BusyStateName = "Busy";

        private string workingDirectoryPath;
        private Action<string> logAction;
        private Dictionary<string, List<DocumentChunk>> fileChunks; 

        public MessageQueue Queue { get; set; }

        public ScannerServiceHandler(MessageQueue queue, string workingDirectoryPath, Action<string> logAction)
        {
            Queue = queue;
            this.workingDirectoryPath = workingDirectoryPath;
            this.logAction = logAction;
            fileChunks = new Dictionary<string, List<DocumentChunk>>();
        }

        public void Handle(Message message)
        {
            logAction($"Recieved '{message}' from {Queue.QueueName}");

            var messageBody = message.Body;
            var state = messageBody as ScannerState;
            if (state != null)
            {
                HandleScannerState(state);
                return;
            }

            var documentChunk = messageBody as DocumentChunk;
            if (documentChunk != null)
            {
                HandleDocumentChunk(documentChunk);
            }
        }

        private void HandleDocumentChunk(DocumentChunk documentChunk)
        {
            logAction($"Recieved document {documentChunk.FileName} chunk #{documentChunk.ChunkNumber} of {documentChunk.TotalChunks}");

            List<DocumentChunk> chunks;
            if (!fileChunks.TryGetValue(documentChunk.FileName, out chunks))
            {
                chunks = new List<DocumentChunk>();
                fileChunks.Add(documentChunk.FileName, chunks);
            }

            chunks.Add(documentChunk);

            if (chunks.Count == documentChunk.TotalChunks)
            {
                logAction($"Received all chunks of {documentChunk.FileName}, writing to file...");
                WriteChunksToFile(chunks);
                logAction($"File {documentChunk.FileName} written.");

                fileChunks.Remove(documentChunk.FileName);
            }
        }

        private void WriteChunksToFile(List<DocumentChunk> chunks)
        {
            using (var fileStream = new FileStream(Path.Combine(workingDirectoryPath, chunks.First().FileName), FileMode.Create))
            {
                foreach (var chunk in chunks.OrderBy(chunk => chunk.ChunkNumber))
                {
                    fileStream.Write(chunk.Data, 0, chunk.Data.Length);
                }
            }
        }

        private void HandleScannerState(ScannerState state)
        {
            logAction($"{Queue.QueueName} is {(state.Status == ScannerStatus.Busy ? BusyStateName : IdleStateName)}, timeout = {state.Configuration.ScanTimeout}");
        }

        public void UpdateConfiguration(ScannerConfiguration configuration)
        {
            Queue.Send(new UpdateConfigurationCommand() {Configuration = configuration});
        }
    }
}
