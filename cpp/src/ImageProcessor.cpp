#include "../include/ImageProcessor.h"
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <iostream>

// Define IMAGEPROCESSOR_EXPORTS when building the DLL
// This should typically be done in the project settings
#define IMAGEPROCESSOR_EXPORTS

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
    ) {
        try {
            std::cout << "ProcessImage_Canny called with:" << std::endl;
            std::cout << "  Image size: " << width << "x" << height << std::endl;
            std::cout << "  ROI: (" << roiX << ", " << roiY << ", " << roiWidth << ", " << roiHeight << ")" << std::endl;
            std::cout << "  Thresholds: " << threshold1 << ", " << threshold2 << ", " << grayscaleThreshold << std::endl;

            // 1. Wrap the full input data in a cv::Mat
            cv::Mat fullImage(height, width, CV_8UC4, inputImageData);

            // 2. Create a cv::Rect for the ROI and perform bounds checking
            cv::Rect roiRect(roiX, roiY, roiWidth, roiHeight);
            
            // Ensure the ROI is within the image boundaries
            roiRect &= cv::Rect(0, 0, width, height);

            if (roiRect.width <= 0 || roiRect.height <= 0) {
                std::cout << "ERROR: Invalid ROI after bounds checking: " << roiRect.width << "x" << roiRect.height << std::endl;
                return -2; // Indicate invalid ROI
            }

            std::cout << "  Final ROI after bounds checking: (" << roiRect.x << ", " << roiRect.y << ", " << roiRect.width << ", " << roiRect.height << ")" << std::endl;

            // 3. Extract the ROI from the full image
            cv::Mat roiImage = fullImage(roiRect);

            // 4. Convert the ROI to grayscale
            cv::Mat grayRoi;
            cv::cvtColor(roiImage, grayRoi, cv::COLOR_BGRA2GRAY);

            // 5. Apply the grayscale threshold to the ROI
            cv::Mat thresholdedRoi;
            cv::threshold(grayRoi, thresholdedRoi, grayscaleThreshold, 255, cv::THRESH_BINARY);

            // 6. Apply the Canny edge detection algorithm on the thresholded ROI
            cv::Mat cannyEdges;
            cv::Canny(thresholdedRoi, cannyEdges, threshold1, threshold2);

            // 7. Copy the result to the output buffer.
            // The output buffer is expected to be the size of the ROI.
            size_t dataSize = cannyEdges.total() * cannyEdges.elemSize();
            std::cout << "  Copying " << dataSize << " bytes to output buffer" << std::endl;
            std::cout << "  Canny result size: " << cannyEdges.cols << "x" << cannyEdges.rows << std::endl;

            memcpy(outputImageData, cannyEdges.data, dataSize);

            std::cout << "ProcessImage_Canny completed successfully" << std::endl;
            return 0; // Success
        }
        catch (const cv::Exception& e) {
            std::cout << "OpenCV exception: " << e.what() << std::endl;
            return -1;
        }
        catch (const std::exception& e) {
            std::cout << "Standard exception: " << e.what() << std::endl;
            return -1;
        }
        catch (...) {
            std::cout << "Unknown exception occurred" << std::endl;
            return -1;
        }
    }
}