using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Test.Services;

public class ItemImageServiceTest
{
    [Theory]
    [InlineData("org1")]
    public void GetItemImageUploadPresignedUrlTest(string orgId)
    {
        var presignedUrl = "https://xxx/thisis/presigned/url.jpg";
        var itemId = Guid.NewGuid().ToString();

        var bucket = Environment.GetEnvironmentVariable("STORAGE_BUCKET")!;
        var contentType = "image/png";

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.GenerateUploadUrl(bucket, It.IsAny<string>(), It.IsAny<TimeSpan>(), contentType)).Returns(presignedUrl);

        var repo = new Mock<IItemImageRepository>();

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.GetItemImageUploadPresignedUrl(orgId, itemId);

        Assert.NotNull(result);

        Assert.NotNull(result.PresignedUrl);
        Assert.Equal(presignedUrl, result.PresignedUrl);
    }

    [Theory]
    [InlineData("org1")]
    public void GetItemImageByIdTest(string orgId)
    {
        var imageUrl = "https://thisis/fake/image-url.jpg";
        var itemImageId = Guid.NewGuid().ToString();

        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.GetItemImageById(itemImageId)).Returns(new MItemImage()
        {
            ImageUrl = imageUrl,
        });

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.GetItemImageById(orgId, itemImageId);

        Assert.NotNull(result);

        Assert.NotNull(result.ImageUrl);
        Assert.Equal(imageUrl, result.ImageUrl);
    }


    [Theory]
    [InlineData("org1")]
    public void AddItemImageObjectNotFoundTest(string orgId)
    {
        var imagePath = "xxx/yyy/zzz.jpg";
        var itemImage = new MItemImage() { ImagePath = imagePath };

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(false);

        var repo = new Mock<IItemImageRepository>();

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.AddItemImage(orgId, itemImage);

        Assert.NotNull(result);
        Assert.Equal("OBJECT_NOT_FOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaa/b/c/d")]
    [InlineData("org1", "hell/o/world")]
    public void AddItemValidateObjectNotFoundTest(string orgId, string? imagePath)
    {
        var itemImage = new MItemImage() { ImagePath = imagePath };
        Google.Apis.Storage.v1.Data.Object storageObj = null!;

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath!)).Returns(storageObj);
        storageUtil.Setup(s => s.IsObjectExist(imagePath!)).Returns(true);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.AddItemImage(itemImage)).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.AddItemImage(orgId, itemImage);

        Assert.NotNull(result);
        Assert.Equal("OBJECT_NOT_FOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaa/b/c/d")]
    [InlineData("org1", "hell/o/world")]
    public void AddItemValidateFileTooBigTest(string orgId, string? imagePath)
    {
        var itemImage = new MItemImage() { ImagePath = imagePath };
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 2 * 1024 * 1024,
            ContentType = "image/png"
        };

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath!)).Returns(storageObj);
        storageUtil.Setup(s => s.IsObjectExist(imagePath!)).Returns(true);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.AddItemImage(itemImage)).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.AddItemImage(orgId, itemImage);

        Assert.NotNull(result);
        Assert.Equal("FILE_TOO_BIG", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaa/b/c/d")]
    [InlineData("org1", "hell/o/world")]
    public void AddItemValidateFileNotPngTest(string orgId, string? imagePath)
    {
        var itemImage = new MItemImage() { ImagePath = imagePath };
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 1000,
            ContentType = "image/jpg"
        };

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath!)).Returns(storageObj);
        storageUtil.Setup(s => s.IsObjectExist(imagePath!)).Returns(true);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.AddItemImage(itemImage)).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.AddItemImage(orgId, itemImage);

        Assert.NotNull(result);
        Assert.Equal("FILE_TYPE_NOT_PNG", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaa/b/c/d")]
    [InlineData("org1", "hell/o/world")]
    public void AddItemValidateFileNotValidPngTest(string orgId, string? imagePath)
    {
        var itemImage = new MItemImage() { ImagePath = imagePath };
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 1000,
            ContentType = "image/png"
        };

        var header = new byte[10];

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath!)).Returns(storageObj);
        storageUtil.Setup(s => s.IsObjectExist(imagePath!)).Returns(true);
        storageUtil.Setup(s => s.PartialDownloadToStream(It.IsAny<string>(), imagePath!, 0, 24)).ReturnsAsync(header);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.AddItemImage(itemImage)).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.AddItemImage(orgId, itemImage);

        Assert.NotNull(result);
        Assert.Equal("NOT_VALID_PNG_FILE", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaa/b/c/d")]
    [InlineData("org1", "hell/o/world")]
    public void AddItemInvalidDimensionTest(string orgId, string? imagePath)
    {
        var itemImage = new MItemImage() { ImagePath = imagePath };
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 1000,
            ContentType = "image/png"
        };

        var header = new byte[24];
        // 1500 decimal
        header[16] = 0x00;
        header[17] = 0x00;
        header[18] = 0x05;
        header[19] = 0xDC;

        // 1500 decimal
        header[20] = 0x00;
        header[21] = 0x00;
        header[22] = 0x05;
        header[23] = 0xDC; 

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath!)).Returns(storageObj);
        storageUtil.Setup(s => s.IsObjectExist(imagePath!)).Returns(true);
        storageUtil.Setup(s => s.PartialDownloadToStream(It.IsAny<string>(), imagePath!, 0, 24)).ReturnsAsync(header);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.AddItemImage(itemImage)).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.AddItemImage(orgId, itemImage);

        Assert.NotNull(result);
        Assert.Equal("INVALID_IMAGE_DIMENSION", result.Status);
    }

    [Theory]
    [InlineData("org1", "")]
    [InlineData("org1", null)]
    public void AddItemImageOkTest(string orgId, string? imagePath)
    {
        var itemImage = new MItemImage() { ImagePath = imagePath };

        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.AddItemImage(itemImage)).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.AddItemImage(orgId, itemImage);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }

    [Theory]
    [InlineData("org1", "this/is/image/path.jpg")]
    public void AddItemImageOkWithImagePathTest(string orgId, string imagePath)
    {
        var itemImage = new MItemImage() { ImagePath = imagePath };
        var header = new byte[24];
        // ตั้งค่าให้ offset 16 = 100
        header[16] = 0x00;
        header[17] = 0x00;
        header[18] = 0x00;
        header[19] = 0x64; // 100 decimal

        // ตั้งค่าให้ offset 20 = 100
        header[20] = 0x00;
        header[21] = 0x00;
        header[22] = 0x00;
        header[23] = 0x64; // 100 decimal

        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 100,
            ContentType = "image/png"
        };

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath)).Returns(storageObj);
        storageUtil.Setup(s => s.PartialDownloadToStream(It.IsAny<string>(), imagePath, 0, 24)).ReturnsAsync(header);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.AddItemImage(itemImage)).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.AddItemImage(orgId, itemImage);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }

    [Theory]
    [InlineData("org1", "this/is/image/path.jpg")]
    public void UpdateItemImageNotFoundTest(string orgId, string imagePath)
    {
        var id = Guid.NewGuid();
        var itemImage = new MItemImage() { ImagePath = imagePath, Id = id };

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.UpdateItemImageById(id.ToString(), itemImage)).Returns((MItemImage?)null);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.UpdateItemImageById(orgId, id.ToString(), itemImage);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "this/is/image/path.jpg")]
    [InlineData("org1", "")]
    public void UpdateItemImageOkTest(string orgId, string imagePath)
    {
        var id = Guid.NewGuid();
        var itemImage = new MItemImage() { ImagePath = imagePath, Id = id };

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.UpdateItemImageById(id.ToString(), itemImage)).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.UpdateItemImageById(orgId, id.ToString(), itemImage);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void DeleteItemImageIdInvalidTest(string orgId)
    {
        var id = "this is invalid uuid";
        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.DeleteItemImageByItemId(orgId, id.ToString());

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "this/is/image/path.jpg")]
    public void DeleteItemImageNotFoundTest(string orgId, string imagePath)
    {
        var id = Guid.NewGuid();
        var itemImage = new MItemImage() { ImagePath = imagePath, Id = id };

        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.DeleteItemImageByItemId(id.ToString())).Returns((List<MItemImage>?)null);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.DeleteItemImageByItemId(orgId, id.ToString());

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "this/is/image/path.jpg")]
    public void DeleteItemImageOkTest(string orgId, string imagePath)
    {
        var id = Guid.NewGuid();
        var itemImage = new MItemImage() { ImagePath = imagePath, Id = id };

        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.DeleteItemImageByItemId(id.ToString())).Returns(
            [
                new MItemImage()
            ]
        );

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.DeleteItemImageByItemId(orgId, id.ToString());

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public void DeleteItemImageByIdInvalidTest(string orgId)
    {
        var id = "this is invalid uuid";
        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.DeleteItemImageById(orgId, id.ToString());

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "this/is/image/path.jpg")]
    public void DeleteItemImageByIdNotFoundTest(string orgId, string imagePath)
    {
        var id = Guid.NewGuid();
        var itemImage = new MItemImage() { ImagePath = imagePath, Id = id };

        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.DeleteItemImageById(id.ToString())).Returns((MItemImage?)null);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.DeleteItemImageById(orgId, id.ToString());

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "this/is/image/path.jpg")]
    public void DeleteItemImageByIdOkTest(string orgId, string imagePath)
    {
        var id = Guid.NewGuid();
        var itemImage = new MItemImage() { ImagePath = imagePath, Id = id };

        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.DeleteItemImageById(id.ToString())).Returns(itemImage);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.DeleteItemImageById(orgId, id.ToString());

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }

 
    [Theory]
    [InlineData("org1", "this/is/image/path")]
    public void GetItemImagesByItemIdOkTest(string orgId, string imagePath)
    {
        var itemId = Guid.NewGuid().ToString();
        var presignedUrl = "https://this/is/presigned/url.jpg";

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.GenerateDownloadUrl(imagePath, It.IsAny<TimeSpan>(), "image/png")).Returns(presignedUrl);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.GetItemImages(It.IsAny<VMItemImage>())).Returns(
            [
                new MItemImage() { ImagePath = imagePath },
                new MItemImage() { ImagePath = imagePath },
            ]);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.GetItemImagesByItemId(orgId, itemId);

        Assert.Equal(2, result.ToArray().Length);
    }
    
    [Theory]
    [InlineData("org1", "this/is/image/path")]
    public void GetItemImagesOkTest(string orgId, string imagePath)
    {
        var presignedUrl = "https://this/is/presigned/url.jpg";
        var param = new VMItemImage();

        var storageUtil = new Mock<IStorageUtils>();
        storageUtil.Setup(s => s.GenerateDownloadUrl(imagePath, It.IsAny<TimeSpan>(), "image/png")).Returns(presignedUrl);

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.GetItemImages(param)).Returns(
            [
                new MItemImage() { ImagePath = imagePath },
                new MItemImage() { ImagePath = imagePath },
            ]);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var result = itemSvc.GetItemImages(orgId, param);

        Assert.Equal(2, result.ToArray().Length);
    }

    [Theory]
    [InlineData("org1")]
    public void GetItemImageCountOkTest(string orgId)
    {
        var param = new VMItemImage();

        var storageUtil = new Mock<IStorageUtils>();

        var repo = new Mock<IItemImageRepository>();
        repo.Setup(s => s.GetItemImageCount(param)).Returns(10);

        var itemSvc = new ItemImageService(repo.Object, storageUtil.Object);
        var cnt = itemSvc.GetItemImageCount(orgId, param);

        Assert.Equal(10, cnt);
    }
}