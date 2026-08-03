1. **Understand & Measure**: The current `Upload` loop runs synchronously (`foreach` calling `UploadFile` which performs synchronous `RestSharp` calls). I'll write a benchmark simulating the `UploadFile` API calls synchronously vs asynchronously using `Task.WhenAll` to establish baseline performance metrics. (I've already run a simple console benchmark showing large improvement from async).
2. **Refactor threading to async/await**: Replace the fragile `Thread.Abort()` based upload cancellation with a `CancellationTokenSource`.
3. **Make `Upload` async**: Change `Upload()` to `UploadAsync(CancellationToken token)`. Change the `foreach` loop to start all tasks and await them using `Task.WhenAll`.
4. **Make `UploadFile` async**: Change `UploadFile(string)` to `UploadFileAsync(string, CancellationToken)`. Use `await _client.ExecuteAsync(request, token)` instead of `.Execute()`.
5. **Verify**: Ensure the code compiles, the test suite passes, and the application builds.
6. **Pre-commit checks**: Run any requested pre-commit steps.
7. **Submit PR**: Detail the performance improvements in the description.
