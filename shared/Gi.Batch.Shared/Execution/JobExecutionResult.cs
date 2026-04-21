namespace Gi.Batch.Shared.Execution
{
    public sealed class JobExecutionResult
    {
        public bool Success { get; }
        public int ExitCode { get; }
        public string Message { get; }

        private JobExecutionResult(bool success, int exitCode, string message)
        {
            Success = success;
            ExitCode = exitCode;
            Message = message ?? string.Empty;
        }

        public static JobExecutionResult Ok(string message = "")
            => new JobExecutionResult(true, 0, message);

        public static JobExecutionResult Fail(int exitCode, string message)
            => new JobExecutionResult(false, exitCode, message);
    }
}
