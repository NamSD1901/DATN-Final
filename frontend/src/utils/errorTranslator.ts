export const translateApiError = (err: any, t: (key: string) => string, defaultKey: string): string => {
  const apiError = err?.response?.data?.message || err?.message || '';

  // Handle specific backend error codes/prefixes
  if (apiError.startsWith('MISSING_PHONE')) {
    return t('errors.MISSING_PHONE');
  }
  if (apiError.startsWith('PET_NOT_FOUND')) {
    return t('errors.PET_NOT_FOUND');
  }
  if (apiError.startsWith('SERVICE_UNAVAILABLE')) {
    return t('errors.SERVICE_UNAVAILABLE');
  }
  if (apiError.startsWith('SLOT_TAKEN')) {
    return t('errors.SLOT_TAKEN');
  }
  
  // If it's a known error but not strictly starting with those keys, or just fallback
  if (apiError) {
    // Optionally we can just return the backend message directly if it's localized or we don't have a mapping
    // But since the goal is to translate backend messages, we check if it matches a key, otherwise fallback
    return apiError;
  }

  // Fallback to default
  return t(`errors.${defaultKey}`);
};
