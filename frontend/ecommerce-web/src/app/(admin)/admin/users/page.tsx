"use client";

import { useState } from "react";
import { changeRole } from "@/lib/api/auth";

const ROLES = ["Customer", "Admin"];

export default function AdminUsersPage() {
  const [userId, setUserId] = useState("");
  const [role, setRole] = useState<"Admin" | "Customer">("Customer");
  const [success, setSuccess] = useState("");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSuccess("");
    setError("");
    const id = parseInt(userId, 10);
    if (isNaN(id) || id <= 0) {
      setError("Please enter a valid user ID.");
      return;
    }
    setSaving(true);
    try {
      await changeRole(id, { role });
      setSuccess(`Role updated to "${role}" for user ID ${id}.`);
      setUserId("");
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Failed to update role");
    } finally {
      setSaving(false);
    }
  };

  return (
    <>
      <h2 className="mb-4">User Management</h2>
      <div className="row">
        <div className="col-md-5">
          <div className="card">
            <div className="card-header fw-bold">Change User Role</div>
            <div className="card-body">
              {success && <div className="alert alert-success">{success}</div>}
              {error && <div className="alert alert-danger">{error}</div>}
              <form onSubmit={handleSubmit}>
                <div className="mb-3">
                  <label className="form-label">User ID</label>
                  <input
                    type="number"
                    min={1}
                    className="form-control"
                    placeholder="Enter user ID…"
                    value={userId}
                    onChange={(e) => setUserId(e.target.value)}
                    required
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">New Role</label>
                  <select
                    className="form-select"
                    value={role}
                    onChange={(e) => setRole(e.target.value as "Admin" | "Customer")}
                  >
                    {ROLES.map((r) => <option key={r} value={r}>{r}</option>)}
                  </select>
                </div>
                <button type="submit" className="btn btn-primary w-100" disabled={saving}>
                  {saving ? "Updating…" : "Update Role"}
                </button>
              </form>
            </div>
          </div>
        </div>
        <div className="col-md-7">
          <div className="alert alert-info">
            <strong>Note:</strong> User listing is not available via API. Use the form on the left to update a specific user&apos;s role by their ID.
          </div>
        </div>
      </div>
    </>
  );
}
