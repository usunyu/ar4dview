//
//  Texture4DR.h
//
//  Copyright 2013 4D View Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#if TARGET_OS_IPHONE
#import <UIKit/UIImage.h>
#else
#import <AppKit/NSImage.h>
#endif


@interface Texture4DR : NSObject

@property short m_width;
@property short m_height;
@property short m_format; //0 = Raw RGB (N/A), 1 = DXT1 (N/A), 2 = PNG, 3 = JPG, 4 = PVRTC(2bpp), 5=ETC1, 6 = PVRTC(4bpp)

//---- For PNG and JPEG formats
#if TARGET_OS_IPHONE
- (UIImage*) decodeImage;                                    //returns nil if wrong format
#else
- (NSImage*) decodeImage;
#endif
- (BOOL)     decodeImageToRGBA:(unsigned char*) bufferRGBA;  //returns NO if wrong format (buffer must be allocated)

//---- For PVRTC and ETC formats
@property char* m_image;       //for PVRTC and ETC: compressed texture buffer
@property int   m_payloadSize; //m_image size

@end
