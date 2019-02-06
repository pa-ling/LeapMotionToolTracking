#include "Marker.h"

Marker::Marker(float x , float y, float r)
{
	this->x = x;
	this->y = y;
	this->r = r;
}

Marker::Marker()
{
	Marker(-1.0, -1.0, -1.0);
}

Marker::Marker(const Marker &marker)
{
	Marker(marker.x, marker.y, marker.r);
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