using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Repositories.Implementation;
using Microsoft.AspNetCore.Http;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class FileRepositoryTests
    {
        // ----------------------------
        // Helper: temp directory
        // ----------------------------
        private static string CreateTempFolder(string name)
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "PayTrackTests",
                name,
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(path);
            return path;
        }

        private static IConfiguration CreateConfig(string path)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Data:FileUploadPath"] = path
                })
                .Build();
        }

        private static IFormFile CreateMockFile(byte[] data, string fileName = "test.pdf")
        {
            return new FormFile(
                new MemoryStream(data),
                0,
                data.Length,
                "file",
                fileName
            );
        }

        // ----------------------------
        // SaveFile
        // ----------------------------
        [Fact]
        public async Task SaveFile_ShouldSaveFile_AndReturnPath()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFile");
            var config = CreateConfig(folder);
            var repo = new FileRepository(config);

            var file = CreateMockFile([1, 2, 3], "test.pdf");

            // Act
            var result = await repo.SaveFile(file, "invoice_123");

            // Assert
            File.Exists(result).Should().BeTrue();

            var content = await File.ReadAllBytesAsync(result);
            content.Should().Equal(1, 2, 3);
        }

        [Fact]
        public async Task SaveFile_ShouldThrow_WhenFileIsNull()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileNull");
            var config = CreateConfig(folder);
            var repo = new FileRepository(config);

            // Act
            Func<Task> act = async () =>
                await repo.SaveFile(null!, "name");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidFileException>()
                .WithMessage("No file uploaded");
        }

        [Fact]
        public async Task SaveFile_ShouldThrow_WhenFileIsEmpty()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileEmpty");
            var config = CreateConfig(folder);
            var repo = new FileRepository(config);

            var file = CreateMockFile([]);

            // Act
            Func<Task> act = async () =>
                await repo.SaveFile(file, "name");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidFileException>()
                .WithMessage("No file uploaded");
        }

        // ----------------------------
        // GetByPath
        // ----------------------------
        [Fact]
        public async Task GetByPath_ShouldReturnFileContent()
        {
            // Arrange
            var folder = CreateTempFolder("GetFile");
            var config = CreateConfig(folder);
            var repo = new FileRepository(config);

            var filePath = Path.Combine(folder, "test.txt");
            var data = new byte[] { 1, 2, 3 };

            await File.WriteAllBytesAsync(filePath, data);

            // Act
            var result = await repo.GetByPath(filePath);

            // Assert
            result.Should().Equal(data);
        }

        [Fact]
        public async Task GetByPath_ShouldThrow_WhenFileNotFound()
        {
            // Arrange
            var folder = CreateTempFolder("MissingFile");
            var config = CreateConfig(folder);
            var repo = new FileRepository(config);

            var fakePath = Path.Combine(folder, "missing.txt");

            // Act
            Func<Task> act = async () =>
                await repo.GetByPath(fakePath);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Could not find receipt.");
        }

        [Fact]
        public async Task GetByPath_ShouldThrow_WhenPathIsOutsideBaseFolder()
        {
            // Arrange
            var folder = CreateTempFolder("UnsafePath");
            var config = CreateConfig(folder);
            var repo = new FileRepository(config);

            const string outsidePath = "/etc/passwd";

            // Act
            Func<Task> act = async () =>
                await repo.GetByPath(outsidePath);

            // Assert
            await act.Should()
                .ThrowAsync<InternalErrorException>()
                .WithMessage("Accessing unallowed path");
        }
    }
}
