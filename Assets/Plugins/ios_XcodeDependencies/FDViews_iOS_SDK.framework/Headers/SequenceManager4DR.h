//
//  SequenceManager4DR.h
//
//  Copyright 2013 4D View Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Decoder4DR.h"


//** Protocol for data receiver
@protocol SequenceManagerOuputDelegate
//On specific callback queue
- (void) didOutputDecoder:(Decoder4DR*) output withIndex:(int)index fromSender:(id)sender;
//On specific callback queue when playback reachs end of range and ends
- (void) didPlaybackEndFrom:(id) sender;
//On specific callback queue when playback reachs end of range and restarts (REVERSE OR RESTART modes)
- (void) didPlaybackLoopFrom:(id) sender;
@end
//**

@interface SequenceManager4DR : NSObject

//-- constructor
//Inits with xml file. Works with local and remote URLs
- (id) initWithDescriptor:(NSURL*)descriptor;
//Inits with url, file format(ex:"model-%05d.4dr") and frame range (firtIndex + nbFrames)
//Works with local and remote URLs
- (id) initWithPath:(NSURL*)path fileFormat:(NSString*)format frameRange:(NSRange)range;
//Inits with file. It will explore other 4dr files in the file directory. Mandatory file template: "model-%05d.4dr"
//Work only with local files
- (id) initWithFile:(NSString*)file;

//-- decoding options
@property BOOL decompressGeometry;                      //Refer to the Decoder4DR clas reference

//-- buffer options
typedef enum {BUFFER_NONE=0, BUFFER_RAW, BUFFER_DECODED} BufferMode;
@property (nonatomic) BufferMode bufferMode;            //Specifies if the buffer only loads or also decodes
@property (nonatomic) int bufferSize;                   //Specifies the buffer size (nbFrames)

//-- cache options
typedef enum {CACHE_NONE=0, CACHE_RAW, CACHE_DECODED} CacheMode;
@property (nonatomic) CacheMode cacheMode;              //Specifies if the buffer keeps the data after reading
- (void) clearBufferAndCache;                           //Release the cache and the buffer (free memory)

//-- playback options
@property (nonatomic) float frameRate;                   //Specifies the playback frame rate
@property (nonatomic) BOOL  realtime;                    //Drops frames when cannot handle the framerate

@property (nonatomic) int playbackStep;                  //Specifies the direction and step  (ex:1 or -1)  of
                                                         //the playback (automatically updated in REVERSE mode)
                                                         //Warning: only -1 and 1 are implemented for the moment!

typedef enum {STOP=0, RESTART, REVERSE} OutOfRangeMode;
@property (nonatomic) OutOfRangeMode outOfRangeMode;     //Specifies what to do when playback ends

//-- playback functions
- (void) startBuffering;            //Fills the buffer from current playback frame to playback direction
- (void) waitBuffering;             //Wait for the current buffer has ended. Works only if sequence
                                    //manager is not playing else returns directly.

- (void) play;                      //Plays and starts automatically the buffering if needed
- (void) pause;
- (void) playPause;
- (void) stop;
- (void) previous;
- (void) next;
- (void) goToFrame:(int) frame;    //set currentFrame to frame
- (void) setActiveRangeFrom:(int) firstFrameId to:(int) lastFrameId;  //Defines the playback range. Frame Id starts at 0 (relative to the sequence)


//-- sequence infos
@property (nonatomic, retain)           NSString*  description;
@property (nonatomic)                   float      floorOffset;
@property (nonatomic, readonly)         int        nbSequenceFrames;   //Number of frames (full range)
@property (nonatomic, readonly)         int        nbActiveFrames;     //Number of frames  (active range)
@property (nonatomic, readonly)         int        maxVerticesNumber;
@property (nonatomic, readonly)         int        maxTrianglesNumber;
@property (nonatomic, readonly)         int        textureSize;
@property (nonatomic, readonly)         NSString*  textureEncoding;

//-- playback infos
@property (nonatomic, readonly)         int        currentFrame;       //Starts at 0 relative to full range
@property (nonatomic, readonly)         BOOL       isPlaying;
@property (nonatomic, readonly)         NSRange    activeRange;        //Active frame range. Frame Ids are relative to full range

//-- advanced infos
@property (nonatomic, readonly)         int        currentIndex;       //Starts at firstIndex of .xml
@property (nonatomic, retain, readonly) NSURL*     path;
@property (nonatomic, retain, readonly) NSString*  fileNameFormat;
@property (nonatomic, readonly)         NSRange    frameRange;
@property (nonatomic, readonly)         float      droppedFrameRate;   //Returns the dropped frame rate (for realtime mode)
typedef enum{EMPTY=0, RAW=1, DECODED=2} CacheState;
@property (nonatomic, readonly)   NSMutableArray*  cacheStates;        //Cache state for each frame


//-- delegate (data receiver) must implement the Sequence4DRDecoderOuputDelegate protocol
@property (assign) id<SequenceManagerOuputDelegate> delegate;
- (void) setDecoderDelegate:(id < SequenceManagerOuputDelegate >)decoderDelegate queue:(dispatch_queue_t)decoderCallbackQueue;

@end
