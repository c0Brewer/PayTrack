using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Repositories.Implementation;
using Microsoft.AspNetCore.Http;
using System.Reflection;

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

        private static IConfiguration CreateConfig(
            string path,
            bool googleDriveEnabled = false,
            string? googleDriveRootFolderId = null,
            string? googleDriveServiceAccountKeyPath = null)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Data:FileUploadPath"] = path,
                    ["GoogleDrive:Enabled"] = googleDriveEnabled.ToString(),
                    ["GoogleDrive:RootFolderId"] = googleDriveRootFolderId,
                    ["GoogleDrive:ServiceAccountKeyPath"] = googleDriveServiceAccountKeyPath
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

        [Fact]
        public async Task SaveFile_ShouldThrow_WhenGoogleDriveIsEnabledAndRootFolderIdIsMissing()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileDriveMissingRootFolder");
            var config = CreateConfig(folder, googleDriveEnabled: true);
            var repo = new FileRepository(config);

            var file = CreateMockFile([1, 2, 3]);

            // Act
            Func<Task> act = async () =>
                await repo.SaveFile(file, "invoice_123");

            // Assert
            await act.Should()
                .ThrowAsync<InternalErrorException>()
                .WithMessage("Google Drive root folder id is not configured.");
        }

        [Fact]
        public async Task SaveFile_ShouldThrow_WhenGoogleDriveIsEnabledAndServiceAccountKeyPathIsMissing()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileDriveMissingKeyPath");
            var config = CreateConfig(folder, googleDriveEnabled: true, googleDriveRootFolderId: "root-folder-id");
            var repo = new FileRepository(config);

            var file = CreateMockFile([1, 2, 3]);

            // Act
            Func<Task> act = async () =>
                await repo.SaveFile(file, "invoice_123");

            // Assert
            await act.Should()
                .ThrowAsync<InternalErrorException>()
                .WithMessage("Google Drive service account key path is not configured.");
        }

        [Fact]
        public async Task SaveFile_ShouldThrow_WhenGoogleDriveIsEnabledAndServiceAccountKeyFileDoesNotExist()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileDriveMissingKeyFile");
            var missingKeyFilePath = Path.Combine(folder, "missing-service-account.json");
            var config = CreateConfig(
                folder,
                googleDriveEnabled: true,
                googleDriveRootFolderId: "root-folder-id",
                googleDriveServiceAccountKeyPath: missingKeyFilePath);
            var repo = new FileRepository(config);

            var file = CreateMockFile([1, 2, 3]);

            // Act
            Func<Task> act = async () =>
                await repo.SaveFile(file, "invoice_123");

            // Assert
            await act.Should()
                .ThrowAsync<InternalErrorException>()
                .WithMessage("Google Drive service account key file could not be found.");
        }

        [Fact]
        public void EscapeDriveQueryValue_ShouldEscapeBackslashesAndSingleQuotes()
        {
            // Arrange
            var method = typeof(FileRepository).GetMethod(
                "EscapeDriveQueryValue",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = method?.Invoke(null, ["folder\\team's invoices"]);

            // Assert
            result.Should().Be("folder\\\\team\\'s invoices");
        }

        [Theory]
        [InlineData("invoice.pdf", "application/pdf")]
        [InlineData("invoice.png", "image/png")]
        [InlineData("invoice.jpg", "image/jpeg")]
        [InlineData("invoice.jpeg", "image/jpeg")]
        [InlineData("invoice.txt", "application/octet-stream")]
        public void GetContentType_ShouldReturnExpectedContentType(string fileName, string expectedContentType)
        {
            // Arrange
            var method = typeof(FileRepository).GetMethod(
                "GetContentType",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = method?.Invoke(null, [fileName]);

            // Assert
            result.Should().Be(expectedContentType);
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
