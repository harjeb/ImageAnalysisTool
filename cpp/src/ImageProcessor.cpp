#include "../include/ImageProcessor.h"
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>

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
            // 1. Wrap the full input data in a cv::Mat
            cv::Mat fullImage(height, width, CV_8UC4, inputImageData);

            // 2. Create a cv::Rect for the ROI and perform bounds checking
            cv::Rect roiRect(roiX, roiY, roiWidth, roiHeight);
            
            // Ensure the ROI is within the image boundaries
            roiRect &= cv::Rect(0, 0, width, height);

            if (roiRect.width <= 0 || roiRect.height <= 0) {
                // ROI is completely outside the image, return an error or handle as needed
                return -2; // Indicate invalid ROI
            }

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
            memcpy(outputImageData, cannyEdges.data, cannyEdges.total() * cannyEdges.elemSize());

            return 0; // Success
        }
        catch (const cv::Exception& e) {
            // In case of an OpenCV error, return a non-zero value to indicate failure.
            return -1;
        }
    }
}