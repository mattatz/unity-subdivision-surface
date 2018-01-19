#ifndef _EASING_INCLUDED_
#define _EASING_INCLUDED_

float ease_linear(float x) {
	return x;
}

float ease_in_quad(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return c*(t/=d)*t + b;
}

float ease_out_quad(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return -c *(t/=d)*(t-2) + b;
}

float ease_in_out_quad(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	if ((t/=d/2) < 1) return c/2*t*t + b;
	return -c/2 * ((--t)*(t-2) - 1) + b;
}

float ease_in_cubic(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return c*(t/=d)*t*t + b;
}

float ease_out_cubic(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return c*((t=t/d-1)*t*t + 1) + b;
}

float ease_in_out_cubic(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	if ((t/=d/2) < 1) return c/2*t*t*t + b;
	return c/2*((t-=2)*t*t + 2) + b;
}

float ease_in_quart(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return c*(t/=d)*t*t*t + b;
}

float ease_out_quart(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return -c * ((t=t/d-1)*t*t*t - 1) + b;
}

float ease_in_out_quart(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	if ((t/=d/2) < 1) return c/2*t*t*t*t + b;
	return -c/2 * ((t-=2)*t*t*t - 2) + b;
}

float ease_in_quint(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return c*(t/=d)*t*t*t*t + b;
}

float ease_out_quint(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return c*((t=t/d-1)*t*t*t*t + 1) + b;
}

float ease_in_out_quint(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	if ((t/=d/2) < 1) return c/2*t*t*t*t*t + b;
	return c/2*((t-=2)*t*t*t*t + 2) + b;
}

float ease_in_sine(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return -c * cos(t/d * (3.14159265359/2)) + c + b;
}

float ease_out_sine(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return c * sin(t/d * (3.14159265359/2)) + b;
}

float ease_in_out_Sine(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return -c/2 * (cos(3.14159265359*t/d) - 1) + b;
}

float ease_in_expo(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return (t==0) ? b : c * pow(2, 10 * (t/d - 1)) + b;
}

float ease_out_expo(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return (t==d) ? b+c : c * (-pow(2, -10 * t/d) + 1) + b;
}

float ease_in_out_expo(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	if (t==0) return b;
	if (t==d) return b+c;
	if ((t/=d/2) < 1) return c/2 * pow(2, 10 * (t - 1)) + b;
	return c/2 * (-pow(2, -10 * --t) + 2) + b;
}

float ease_in_circ(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return -c * (sqrt(1 - (t/=d)*t) - 1) + b;
}

float ease_out_circ(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	return c * sqrt(1 - (t=t/d-1)*t) + b;
}

float ease_in_out_circ(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	if ((t/=d/2) < 1) return -c/2 * (sqrt(1 - t*t) - 1) + b;
	return c/2 * (sqrt(1 - (t-=2)*t) + 1) + b;
}

float ease_in_elastic(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	float s=1.70158;float p=0;float a=c;
	if (t==0) return b;  if ((t/=d)==1) return b+c;  if (p == 0) p=d*.3;
	if (a < abs(c)) { a=c; s=p/4; }
	else s = p/(2*3.14159265359) * asin (c/a);
	return -(a*pow(2,10*(t-=1)) * sin( (t*d-s)*(2*3.14159265359)/p )) + b;
}

float ease_out_elastic(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	float s=1.70158;float p=0;float a=c;
	if (t==0) return b;  if ((t/=d)==1) return b+c;  if (p ==0) p=d*.3;
	if (a < abs(c)) { a=c; s=p/4; }
	else s = p/(2*3.14159265359) * asin (c/a);
	return a*pow(2,-10*t) * sin( (t*d-s)*(2*3.14159265359)/p ) + c + b;
}

float ease_in_out_elastic(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	float s=1.70158;float p=0;float a=c;
	if (t==0) return b;  if ((t/=d/2)==2) return b+c;  if (p ==0) p=d*(.3*1.5);
	if (a < abs(c)) { a=c; s=p/4; }
	else s = p/(2*3.14159265359) * asin (c/a);
	if (t < 1) return -.5*(a*pow(2,10*(t-=1)) * sin( (t*d-s)*(2*3.14159265359)/p )) + b;
	return a*pow(2,-10*(t-=1)) * sin( (t*d-s)*(2*3.14159265359)/p )*.5 + c + b;
}

float ease_in_back(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	float s = 1.70158;
	return c*(t/=d)*t*((s+1)*t - s) + b;
}

float ease_out_back(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	float s = 1.70158;
	return c*((t=t/d-1)*t*((s+1)*t + s) + 1) + b;
}

float ease_in_out_back(float x) {
	float t = x; float b = 0; float c = 1; float d = 1;
	float s = 1.70158;
	if ((t/=d/2) < 1) return c/2*(t*t*(((s*=(1.525))+1)*t - s)) + b;
	return c/2*((t-=2)*t*(((s*=(1.525))+1)*t + s) + 2) + b;
}

float ease_out_bounce(float x, float t, float b, float c, float d) {
	if ((t/=d) < (1/2.75)) {
		return c*(7.5625*t*t) + b;
	} else if (t < (2/2.75)) {
		return c*(7.5625*(t-=(1.5/2.75))*t + .75) + b;
	} else if (t < (2.5/2.75)) {
		return c*(7.5625*(t-=(2.25/2.75))*t + .9375) + b;
	} else {
		return c*(7.5625*(t-=(2.625/2.75))*t + .984375) + b;
	}
}

float ease_out_bounce(float x){
	return ease_out_bounce(x, x, 0, 1, 1);
}

float ease_in_bounce(float x, float t, float b, float c, float d) {
	return c - ease_out_bounce (x, d-t, 0, c, d) + b;
}

float ease_in_bounce(float x){
	return ease_in_bounce(x, x, 0, 1, 1);
}

float ease_in_out_bounce(float x, float t, float b, float c, float d) {
	if (t < d/2) return ease_in_bounce (x, t*2, 0, c, d) * .5 + b;
	return ease_out_bounce (x, t*2-d, 0, c, d) * .5 + c*.5 + b;
}

float ease_in_out_bounce(float x){
	return ease_in_out_bounce(x, x, 0, 1, 1);
}

#endif // _EASING_INCLUDED_