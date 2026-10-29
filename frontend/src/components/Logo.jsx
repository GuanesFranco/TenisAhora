import React from 'react';

/**
 * Logotipo Vectorial de Tenis Ahora
 * Diseño minimalista con la costura geométrica de pelota de tenis profesional
 */
export function TennisMark({ size = 26, color = 'var(--primary)' }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 32 32"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      style={{ display: 'inline-block', verticalAlign: 'middle', flexShrink: 0 }}
    >
      <circle cx="16" cy="16" r="14" stroke={color} strokeWidth="2" opacity="0.9" />
      <path
        d="M5.5 9.5C9.5 13 11 18.5 10 24.5"
        stroke={color}
        strokeWidth="1.8"
        strokeLinecap="round"
      />
      <path
        d="M26.5 22.5C22.5 19 21 13.5 22 7.5"
        stroke={color}
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </svg>
  );
}

export function Logo({ size = 'md', subtitle = true }) {
  const iconSizes = { sm: 20, md: 26, lg: 32 };
  const currentSize = iconSizes[size] || 26;

  return (
    <div style={{ display: 'inline-flex', alignItems: 'center', gap: 10 }}>
      <div
        style={{
          width: currentSize + 8,
          height: currentSize + 8,
          borderRadius: 8,
          background: 'var(--primary-subtle)',
          border: '1px solid var(--primary-subtle-border)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <TennisMark size={currentSize} color="var(--primary)" />
      </div>

      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <div style={{
          fontWeight: 700,
          fontSize: size === 'lg' ? '1.3rem' : size === 'sm' ? '0.95rem' : '1.1rem',
          letterSpacing: '-0.02em',
          lineHeight: 1.15,
          color: 'var(--text-main)',
        }}>
          TENIS <span style={{ color: 'var(--primary)' }}>AHORA</span>
        </div>
        {subtitle && (
          <span style={{
            fontSize: '0.625rem',
            color: 'var(--text-dim)',
            textTransform: 'uppercase',
            letterSpacing: '0.06em',
            fontWeight: 600,
            marginTop: 2
          }}>
            El Deporte Saludable
          </span>
        )}
      </div>
    </div>
  );
}

export default Logo;
