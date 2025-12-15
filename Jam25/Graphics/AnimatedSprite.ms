namespace Jam25.Graphics

u8 AnimationGraphID
{
	None: 0
}

u8 SpriteTag
{
    Walking: 0
	Running: 1
    Jumping: 2
    AnimationFinished: 3
	Throwing: 4
	Hurt: 5
}

variable AnimatedSprite
{
	SpriteID SpriteId
	AnimationGraphID AnimationGraphId
	string? CurrentNode

	float32 ScaleX: 1
	float32 ScaleY: 1
	bool IsFacingRight: true

	float32 TotalElapsed
	i32 Frame
	bool IsPaused
	bool IsLooping

	float32 Rotation
	float32 Depth
	float32 Opacity: 1

    SpriteTag[] Tags
}
