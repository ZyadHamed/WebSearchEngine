using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeleniumBaseApi.Classes;

public class PythonRunner
{
    public static async Task<(string output, string errorOutput, int exitCode)> RunPythonScriptAsync(string inputData)
    {   
        // We need to know where is the script an where is the python.exe that will run that script
        // gets the root directory where "SeleniumBaseApi" and "SbEnv" are
        string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..");
        
        // gets the full path because it depends on where the code is. I didn't want it to be hard coded
        // the full path is "c:\\user\\..." becuase the relative path didn't work with me, I don't know why
        string workingDirectory = Path.GetFullPath(baseDirectory);

        // Won't change. Don't change it
        string pythonExecutableRelativePath = Path.Combine("SbEnv", ".venv", "Scripts", "python.exe");

        // here where you could edit the relative path. It's hard coded becuase it's likely won't change
        string scriptRelativePath = Path.Combine("SbEnv", "sb_script.py"); //change this to change the script file

        // merges the root both from above with the Exe relative path
        string actualPythonExePath = Path.Combine(workingDirectory, pythonExecutableRelativePath);

        // the process that the cmd will ren to execute the script
        var startInfo = new ProcessStartInfo
        {
            FileName = actualPythonExePath,
            Arguments = $"{scriptRelativePath} \"{inputData.Replace("\"", "\\\"")}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory // leave this alone
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            StringBuilder outputBuilder = new StringBuilder();
            StringBuilder errorBuilder = new StringBuilder();

            //we want the output to be read simultaniously so it goes between them back and forth
            using (var outputWaitHandle = new ManualResetEventSlim(false))
            using (var errorWaitHandle = new ManualResetEventSlim(false))
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null) // A null value indicates the end of the stream
                    {
                        outputWaitHandle.Set(); // Signal that output reading is complete
                    }
                    else
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                // same as above but it reads the error massege
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // wait for the process to exit AND for the output/error streams to be fully read
                    // this is important to make sure that we have captured all data
                    await Task.WhenAll(
                        Task.Run(() => process.WaitForExit()),
                        Task.Run(() => outputWaitHandle.Wait()),
                        Task.Run(() => errorWaitHandle.Wait())
                    );
                    
                    // builds the output and input strings
                    return (outputBuilder.ToString().Trim(), errorBuilder.ToString().Trim(), process.ExitCode);
                }
                catch (Exception ex)
                {
                    return (string.Empty, $"Failed to run Python script: {ex.Message}", -1);
                }
            }
        }
    }
}