using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Image
{
    public class ImageComposerService : IImageComposerService
    {
        /// <summary>
        /// Number of pixels which are both placed continuously in the image and stored continuously in the raw byte data.
        /// </summary>
        private readonly uint stride = 4;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="tileSize"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        public void ReplaceImageTile(TgvImage destination, TgvImage source, uint tileSize, uint column, uint row)
        {
            uint xPos = tileSize * column;
            uint yPos = tileSize * row;

            ReplaceImagePart(destination, source, xPos, yPos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        public void ReplaceImagePart(TgvImage destination, TgvImage source, uint xPos, uint yPos)
        {
            if (destination.MipMaps.Count == 0 || source.MipMaps.Count == 0)
            {
                return;
            }

            if (xPos >= destination.Height || xPos < 0 ||
                yPos >= destination.Width || yPos < 0)
            {
                return;
            }

            var targetImage = GetBiggestMipMap(destination);
            var sourceImage = GetBiggestMipMap(source);

            //Pomyśleć nad odczytywaniem rozmiarów z obiektu mipMapy
            var targetRectangleContent = ConvertContentToRectangleArray(targetImage.Content, destination.Width, destination.Height);
            var sourceRectangleContent = ConvertContentToRectangleArray(sourceImage.Content, source.Width, source.Height);

            for (int x = 0; x < source.Width; ++x)
            {
                for (int y = 0; y < source.Height; ++y)
                {
                    if (xPos + x < destination.Width &&
                        yPos + y < destination.Height)
                    {
                        targetRectangleContent[xPos + x, yPos + y] = sourceRectangleContent[x, y];
                    }
                }
            }

            targetImage.Content = ConvertContentToLinearArray(targetRectangleContent);
            sourceImage.Content = ConvertContentToLinearArray(sourceRectangleContent);
        }

        private TgvMipMap GetBiggestMipMap(TgvImage image)
        {
            var result = image
                .MipMaps
                .ToArray()
                .OrderBy(x => x.Size)
                .Last();

            return result;
        }

        /// <summary>
        /// Converts the raw Tgv image stored in the linear byte array to the two dimensional byte array with a byte order
        /// matching the pixel placement in the image seen as on the screen.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private byte[,] ConvertContentToRectangleArray(byte[] content, uint width, uint height)
        {
            byte[,] result = new byte[width, height];

            //The Block term is used to describe a rectangular area of an image, defined by the image width and the
            //height equal to the amount of pixels which belongs to the one column and are stored in the byte array continuously (stride).
            uint BlockCount = (uint)Math.Ceiling(height / (double)stride);

            for (uint i = 0; i < content.LongLength; ++i)
            {
                uint passedBlocks = i / width / stride;
                uint currentBlock = passedBlocks + 1;
                uint currentBlockStride = stride;
                if (currentBlock == BlockCount)
                {
                    currentBlockStride = height % stride == 0 ? stride : height % stride;
                }

                uint x = (i - (passedBlocks * width * stride)) / currentBlockStride;
                uint y = (i % currentBlockStride) + (passedBlocks * stride);

                result[x, y] = content[i];
            }

            return result;
        }

        /// <summary>
        /// Converts the raw Tgv image given in the form of the rectangle array, 
        /// to the linear byte array, with byte order as in the DDS file.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private byte[] ConvertContentToLinearArray(byte[,] content)
        {
            byte[] result = new byte[content.LongLength];

            uint width = (uint)content.GetLength(0);
            uint height = (uint)content.GetLength(1);
            uint BlockCount = (uint)Math.Ceiling(height / (double)stride);
            uint resultIndexer = 0;

            for (uint currentBlock = 0; currentBlock < BlockCount; ++currentBlock)
            {
                for (uint x = 0; x < width; ++x)
                {
                    for (uint y = 0; y < stride; ++y)
                    {
                        //Sprawdzenie czy indeks y nie wykroczył poza granice, w przypadku obrazów o romiarze nie będącym wielokrotnością liczby stride (4).
                        //uint passedBlocks = (((long)currentBlock - 1) < 0 ? 0 : currentBlock - 1);
                        if ((y + (currentBlock * stride)) < height)
                        {
                            result[resultIndexer++] = content[x, y + (currentBlock * stride)];
                        }
                    }
                }
            }

            return result;
        }

        #region Obsolete
        //public void ReplaceImageTile(TgvImage destination, TgvImage source, uint tileSize, uint column, uint row)
        //{
        //    if (destination.MipMaps.Count == 0 || source.MipMaps.Count == 0)
        //    {
        //        return;
        //    }

        //    uint tilesPerLine = destination.Width / tileSize;
        //    if (column >= tilesPerLine || row >= tilesPerLine)
        //    {
        //        return;
        //    }

        //    //For now, it only does replacement on the biggest mipmaps, others are omitted.
        //    var targetImageContent = destination
        //        .MipMaps
        //        .ToArray()
        //        .OrderBy(x => x.Size)
        //        .Last()
        //        .Content;

        //    var sourceImageContent = source
        //        .MipMaps
        //        .ToArray()
        //        .OrderBy(x => x.Size)
        //        .Last()
        //        .Content;

        //    //BlockksSize * BlockkNumber = TotalImageLength
        //    uint Blockksize = stride * source.Width;
        //    uint BlockksCount = (source.Height * source.Width) / Blockksize; //Bloack count per image
        //    uint targetOffset = (column * Blockksize) + (row * tilesPerLine * Blockksize * BlockksCount);

        //    for (uint i = 0; i < BlockksCount; ++i)
        //    {
        //        for (uint j = 0; j < Blockksize; ++j)
        //        {
        //            //Po zapisaniu każdego bloku, trzeba przejechać całą szerokość obrazu docelowego żeby móc zapisywać kolejne bloki
        //            //do tego samego wewnetrzneo obrazu lod - ((lodImagesPerLine - 1) * i * BlockkSize);
        //            uint targetIndex = targetOffset + (i * Blockksize) + ((tilesPerLine - 1) * i * Blockksize) + j;
        //            uint sourceIndex = (i * Blockksize) + j;

        //            if (sourceIndex >= sourceImageContent.Length ||
        //                targetIndex >= targetImageContent.Length)
        //            {
        //                return;
        //            }

        //            targetImageContent[targetIndex] = sourceImageContent[sourceIndex];
        //        }
        //    }
        //}


        //public void ReplaceImagePartObsolete(TgvImage destination, TgvImage source, uint xPos, uint yPos)
        //{
        //    if (destination.MipMaps.Count == 0 || source.MipMaps.Count == 0)
        //    {
        //        return;
        //    }

        //    if (xPos > destination.Height || yPos > destination.Width)
        //    {
        //        return;
        //    }

        //    //For now, it only does replacement in the biggest mipmaps, others are omitted.
        //    var targetImageContent = GetBiggestMipMap(destination);
        //    var sourceImageContent = GetBiggestMipMap(source);

        //    var expandedSource = ConvertContentToRectangleArray(sourceImageContent, 320, 160); // < do not hardcode these values
        //    var linearSource = ConvertContentToLinearArray(expandedSource);

        //    bool areEqual = WargameModInstaller.Utilities.MiscUtilities.ComparerByteArrays(sourceImageContent, linearSource);

        //    sourceImageContent = linearSource;

        //    uint targetOffset = xPos * yPos;

        //    //Piksele obrazu są ułożone w ciągu bajtów czwórkami, tak że początek czwórki ma współrzędne [0,0] w przestrzeni obrazka
        //    //a koniec czwórki ma [0,3]. Kolejność składowych współrzędnych odpowiada kolejności w układzie kartezjańskim.
        //    //i - odpowiada za rozłożenie bloków powstających z rołożenia j i k (stride * sourceWidth) na całą wysokość.
        //    //j - odpowiada za rozłożenie czwórek na całą szerokość
        //    //k - odpowiada za numerację czwórek pikseli (stride)

        //    for (uint i = 0; i < source.Height / stride; ++i)
        //    {
        //        //Zmienne określające offset długości jaki trzeba dodać w poszczególnych ciągach bajtów, aby trafić 
        //        //po przejściu jednego bloku obrazu, do poczatku następnego bloku obrazu roboczego.
        //        uint destNextBlockOffset = (i * destination.Width * stride); // czy tu napewno powinno być samo dest width bez source width minus...?
        //        uint sourceNextBlockOffset = (i * source.Width * stride);

        //        for (uint j = 0; j < source.Width; ++j)
        //        {
        //            for (uint k = 0; k < stride; ++k)
        //            {
        //                uint targetIndex = targetOffset + k + (stride * j) + destNextBlockOffset;
        //                uint sourceIndex = k + (stride * j) + sourceNextBlockOffset;

        //                if (sourceIndex >= sourceImageContent.Length ||
        //                    targetIndex >= targetImageContent.Length)
        //                {
        //                    return;
        //                }

        //                targetImageContent[targetIndex] = sourceImageContent[sourceIndex];
        //            }
        //        }
        //    }
        //} 
        #endregion //Obsolete

    }

}
