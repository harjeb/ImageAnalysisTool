// 文件名: ImageProcessor.h
#pragma once
#ifdef IMAGEPROCESSOR_EXPORTS
#define API __declspec(dllexport)
#else
#define API __declspec(dllimport)
#endif

extern "C" {
    API int ProcessImage_Canny(
        unsigned char* inputImageData,
        unsigned char* outputImageData,
        int width,
        int height,
        double threshold1,
        double threshold2,
        int grayscaleThreshold,
        int roiX,
        int roiY,
        int roiWidth,
        int roiHeight
    );
}