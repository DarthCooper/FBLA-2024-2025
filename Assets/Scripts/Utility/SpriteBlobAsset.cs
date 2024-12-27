using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct SpriteBlob : IComponentData
{
    public int Width;
    public int Height;
    public BlobArray<float> Pixels;
}
public class SpriteBlobAsset : MonoBehaviour
{
    public Sprite sprite;

    void Start()
    {
        BlobAssetReference<SpriteBlob> spriteBlob = CreateSpriteBlob(sprite);
    }

    public BlobAssetReference<SpriteBlob> CreateSpriteBlob(Sprite sprite)
    {
        // Get the texture from the sprite
        Texture2D texture = sprite.texture;

        // Prepare a BlobBuilder to create a BlobAsset
        BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
        ref SpriteBlob spriteBlob = ref blobBuilder.ConstructRoot<SpriteBlob>();

        // Store the width and height of the sprite
        spriteBlob.Width = texture.width;
        spriteBlob.Height = texture.height;

        // Extract the pixel data from the texture (RGBA values)
        Color32[] pixels = texture.GetPixels32(); // Get RGBA values
        int pixelCount = pixels.Length;

        // Create a BlobArray for the pixel data (4 bytes per pixel: RGBA)
        var pixelDataBlobArray = blobBuilder.Allocate(ref spriteBlob.Pixels, pixelCount * 4);

        // Fill the BlobArray with pixel data
        for (int i = 0; i < pixelCount; i++)
        {
            byte r = pixels[i].r;
            byte g = pixels[i].g;
            byte b = pixels[i].b;
            byte a = pixels[i].a;

            // Store RGBA bytes in the BlobArray
            pixelDataBlobArray[i * 4] = r;
            pixelDataBlobArray[i * 4 + 1] = g;
            pixelDataBlobArray[i * 4 + 2] = b;
            pixelDataBlobArray[i * 4 + 3] = a;
        }

        // Create the BlobAssetReference
        BlobAssetReference<SpriteBlob> blobAsset = blobBuilder.CreateBlobAssetReference<SpriteBlob>(Allocator.Persistent);

        // Clean up and return the BlobAssetReference
        blobBuilder.Dispose();
        return blobAsset;
    }
}
