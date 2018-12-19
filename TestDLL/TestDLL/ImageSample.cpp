/******************************************************************************\
* Copyright (C) 2012-2017 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

#undef __cplusplus

#include "ImageSample.h"
#include <stdio.h>
#include <stdlib.h>

#include "LeapC.h"
#include "ExampleConnection.h"

void* image_buffer = NULL;
uint64_t image_size = 0;
uint32_t image_width = 0;
uint32_t image_height = 0;

/** Callback for when an image is available. */
static void OnImage(const LEAP_IMAGE_EVENT *imageEvent){
	const LEAP_IMAGE_PROPERTIES* properties = &imageEvent->image[0].properties;
	if (properties->bpp != 1) {
		return;
	}

	if (properties->width*properties->height != image_size) {
		void* prev_image_buffer = image_buffer;
		image_width = properties->width;
		image_height = properties->height;
		image_size = image_width * image_height;
		image_buffer = malloc(2 * image_size);
		if (prev_image_buffer) {
			free(prev_image_buffer);
		}
	}

	memcpy(image_buffer, (char*)imageEvent->image[0].data + imageEvent->image[0].offset, image_size);
	memcpy((char*)image_buffer + image_size, (char*)imageEvent->image[1].data + imageEvent->image[1].offset, image_size);
}

void getImage(void* image, int* width, int* height) {
  ConnectionCallbacks.on_image = &OnImage;

  LEAP_CONNECTION *connection = OpenConnection();
  LeapSetPolicyFlags(*connection, eLeapPolicyFlag_Images, 0);

  while (!IsConnected) {
	  millisleep(250);
  }
  printf("Connected.\n");

  *width = image_width;
  *height = image_height;
  memcpy(image, image_buffer, image_width * image_height);
}
