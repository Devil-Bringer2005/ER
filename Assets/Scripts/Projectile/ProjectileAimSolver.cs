using UnityEngine;

/// <summary>
/// Solves the standard "lead the target" firing solution: given a moving
/// target and a projectile speed, find the position the target will be at
/// when a projectile fired now would actually reach it.
/// </summary>
public static class ProjectileAimSolver
{
    public static Vector3 PredictInterceptPosition(
        Vector3 shooterPosition,
        Vector3 targetPosition,
        Vector3 targetVelocity,
        float projectileSpeed)
    {
        Vector3 toTarget = targetPosition - shooterPosition;

        float a = Vector3.Dot(targetVelocity, targetVelocity) - projectileSpeed * projectileSpeed;
        float b = 2f * Vector3.Dot(toTarget, targetVelocity);
        float c = Vector3.Dot(toTarget, toTarget);

        float t = SolveInterceptTime(a, b, c);
        return targetPosition + targetVelocity * t;
    }

    private static float SolveInterceptTime(float a, float b, float c)
    {
        // Target speed ~= projectile speed: quadratic degenerates to linear.
        if (Mathf.Abs(a) < 0.0001f)
        {
            if (Mathf.Abs(b) < 0.0001f) return 0f;
            return Mathf.Max(-c / b, 0f);
        }

        float discriminant = b * b - 4f * a * c;
        if (discriminant < 0f)
        {
            // No real solution - target is outrunning the projectile.
            // Fall back to aiming at their current position.
            return 0f;
        }

        float sqrtDisc = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDisc) / (2f * a);
        float t2 = (-b - sqrtDisc) / (2f * a);

        float t = Mathf.Min(t1, t2);
        if (t < 0f) t = Mathf.Max(t1, t2);
        if (t < 0f) t = 0f;

        return t;
    }
}