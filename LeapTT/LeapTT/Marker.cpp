#include "Marker.h"

Marker::Marker(float x , float y, float r)
{
	this->x = x;
	this->y = y;
	this->r = r;
}

Marker::Marker()
{
	this->x = -1.0;
	this->y = -1.0;
	this->r = -1.0;
}

Marker::Marker(const Marker &marker)
{
	this->x = marker.x;
	this->y = marker.y;
	this->r = marker.r;
}

Marker::~Marker()
{
	//nothing to do
}

float Marker::getX() {
	return this->x;
}

void Marker::setX(float x) {
	this->x = x;
}

float Marker::getY() {
	return this->y;
}

void Marker::setY(float y) {
	this->y = y;
}

float Marker::getR() {
	return this->r;
}

void Marker::setR(float r) {
	this->r = r;
}