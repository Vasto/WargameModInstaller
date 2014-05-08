using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Image
{
    //To do: Add support for the MipMaps parts replacing

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
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        public void ReplaceImagePart(TgvImage target, TgvImage source, uint xPos, uint yPos)
        {
            //Jeśli chodzi o podmienianie częśći obrazka w mipMapach, warunkiem było by że użytkownik zapewnia obrazek źródłowy posiadający mipMapy
            //W takiej sytuacji trzeba było by obliczać dla którego nr mipmapy obrazka z siatką (lods orygianlny) możliwa była by podmiania z wykorzystaniem
            //mipmap obrazka źródłowego od użytkownika...

            if (target.MipMaps.Count == 0 || source.MipMaps.Count == 0)
            {
                return;
            }

            if (xPos >= target.Width || xPos < 0 ||
                yPos >= target.Height || yPos < 0)
            {
                return;
            }

            var sortedSourceMips = source.MipMaps.OrderByDescending(x => x.MipSize).ToArray();
            var sortedTargettMips = target.MipMaps.OrderByDescending(x => x.MipSize).ToArray();

            for (int i = 0; (i < sortedSourceMips.Length) && (i < sortedTargettMips.Length); ++i)
            {
                var sourceMip = sortedSourceMips[i];
                var targetMip = sortedTargettMips[i];

                //1. Przeskalować pozycje umieszczenia
                //2. Spr czy pozycje umieszczenia są poprawne
                //3. ocenić kiedy nie można / staję sie niebezpieczne dalsze przepisywnia mipmap przy jednoczesnym znikomym rezultacie.

                uint mipScale = (uint)Math.Max(1, 2 << (i - 1));//Przenieśc to do jakiego utilies
                uint targetMipXPos = xPos / mipScale;
                uint targetMipYPos = yPos / mipScale;

                if (targetMipXPos >= 0 && targetMipXPos < targetMip.MipWidth &&
                    targetMipYPos >= 0 && targetMipYPos < targetMip.MipHeight)
                {
                    ReplaceMipMapPart(targetMip, sourceMip, targetMipXPos, targetMipYPos);
                }
            }
        }

        private void ReplaceMipMapPart(TgvMipMap target, TgvMipMap source, uint xPos, uint yPos)
        {
            var targetRectangleContent = ConvertContentToRectangleArray(target.Content, target.MipWidth, target.MipHeight);
            var sourceRectangleContent = ConvertContentToRectangleArray(source.Content, source.MipWidth, source.MipHeight);

            for (int x = 0; x < source.MipWidth; ++x)
            {
                for (int y = 0; y < source.MipHeight; ++y)
                {
                    if (xPos + x < target.MipWidth && yPos + y < target.MipHeight)
                    {
                        targetRectangleContent[xPos + x, yPos + y] = sourceRectangleContent[x, y];
                    }
                }
            }

            target.Content = ConvertContentToLinearArray(targetRectangleContent);
            source.Content = ConvertContentToLinearArray(sourceRectangleContent);
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
                    currentBlockStride = height % stride == 0 ? stride :  height % stride;
                }

                uint passedBlockStrideMultiplied = passedBlocks * stride;
                uint x = (i - (passedBlockStrideMultiplied * width)) / currentBlockStride;
                uint y = (i % currentBlockStride) + passedBlockStrideMultiplied;

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
                uint blockStrideMultiplied = currentBlock * stride;

                for (uint x = 0; x < width; ++x)
                {
                    for (uint y = 0; y < stride; ++y)
                    {
                        //Sprawdzenie czy indeks y nie wykroczył poza granice, w przypadku obrazów o romiarze nie będącym wielokrotnością liczby stride (4).
                        //uint passedBlocks = (((long)currentBlock - 1) < 0 ? 0 : currentBlock - 1);
                        if ((y + (blockStrideMultiplied)) < height)
                        {
                            result[resultIndexer++] = content[x, y + blockStrideMultiplied];
                        }
                    }
                }
            }

            return result;
        }

    }

}
