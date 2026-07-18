# API Versioning Policy

## Purpose

The TMS API uses URL-based versioning to allow new features to be introduced without breaking existing clients.

---

## Breaking Changes

The following require a new API version:

- Removing a response field
- Renaming a response field
- Changing the meaning or data type of a field
- Tightening validation rules
- Changing HTTP status codes
- Changing default sorting or filtering behavior

---

## Non-Breaking Changes

The following may be added without creating a new version:

- New optional response fields
- New endpoints
- New optional query parameters
- Performance improvements
- Internal implementation changes

---

## Deprecation Policy

When a new API version is released:

- The previous version is marked as deprecated.
- Responses include Deprecation, Sunset, and Link headers.
- Deprecated versions remain supported for at least six months before removal.

---

## Communication

Version changes are communicated through:

- HTTP Deprecation headers
- Sunset headers
- Successor-version Link headers
- CHANGELOG updates
- Team notifications

---

## Version Migration

Clients may migrate directly from any older version to the latest supported version without upgrading through every intermediate release.