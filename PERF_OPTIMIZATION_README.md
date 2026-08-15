# Performance Optimization Task

## Analysis
The task described the file `uploader/uploader/UploadForm.cs` line 120 containing a synchronous network request loop:
```csharp
            foreach (var file in _filesToUpload)
            {
                UploadFile(file);
            }
```

However, after investigating the `master` branch of the codebase, this loop has already been rewritten to use an asynchronous approach with `Task.WhenAll` to launch the network requests simultaneously:
```csharp
            var tasks = new List<Task>();
            foreach (var file in _filesToUpload)
            {
                tasks.Add(UploadFileAsync(file, token));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
```

## Conclusion
The requested performance improvement (to use asynchronous API calls `Task.WhenAll`) is already present in the codebase. The upload process utilizes a CancellationTokenSource for cooperative cancellation, and handles OperationCanceledException correctly. As a result, no further changes to this code path were required. The optimization effectively reduces time spent uploading multiple files in parallel compared to the legacy sequential execution.
