sed -i 's/_uploadTask = UploadAsync(_cancellationTokenSource.Token);/_uploadTask = Task.Run(async () => await UploadAsync(_cancellationTokenSource.Token));/g' uploader/uploader/UploadForm.cs
