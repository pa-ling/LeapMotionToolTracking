#pragma once
class Marker
{
	float x;
	float y;
	float r;
public:
	Marker(float x, float y, float r);
	Marker();
	Marker(const Marker &marker);
	~Marker();
	float getX();
	void setX(float x);
	float getY();
	void setY(float y);
	float getR();
	void setR(float r);
};

