-- PostgreSQL schema for ResourcePlanningAccelist
-- Status values use underscores in the database where the UI uses hyphenated labels.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

DO $$
BEGIN
  CREATE TYPE user_role AS ENUM ('marketing', 'pm', 'gm', 'hr', 'employee');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE skill_category AS ENUM ('technical', 'soft', 'business');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE project_status AS ENUM (
    'draft',
    'submitted',
    'approved',
    'rejected',
    'assigned',
    'in_progress',
    'completed',
    'cancelled'
  );
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE project_risk_level AS ENUM ('low', 'medium', 'high');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE experience_level AS ENUM ('junior', 'mid', 'senior');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE priority_level AS ENUM ('low', 'medium', 'high');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE assignment_status AS ENUM (
    'pending',
    'approved',
    'rejected',
    'accepted',
    'in_progress',
    'completed',
    'cancelled'
  );
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE assignment_review_status AS ENUM ('pending', 'approved', 'rejected');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE contract_status AS ENUM ('active', 'extended', 'terminated', 'expired');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE decision_type AS ENUM ('extend_contract', 'terminate_contract', 'hire_resource');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE decision_status AS ENUM ('pending', 'executed', 'clarification_requested');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE notification_type AS ENUM ('assignment', 'change', 'alert', 'feedback');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE review_decision AS ENUM ('approved', 'rejected', 'revision_requested');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$
BEGIN
  CREATE TYPE workload_status AS ENUM ('available', 'moderate', 'busy', 'overloaded');
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$;

CREATE TABLE IF NOT EXISTS departments (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL UNIQUE,
  description text,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS app_users (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  email text NOT NULL UNIQUE,
  full_name text NOT NULL,
  avatar_url text,
  role user_role NOT NULL,
  department_id uuid REFERENCES departments(id) ON DELETE SET NULL,
  is_active boolean NOT NULL DEFAULT true,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS employees (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL UNIQUE REFERENCES app_users(id) ON DELETE CASCADE,
  employee_code text UNIQUE,
  phone text,
  location text,
  department_id uuid REFERENCES departments(id) ON DELETE SET NULL,
  job_title text NOT NULL,
  status text NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'inactive', 'resigned')),
  hire_date date,
  availability_percent numeric(5,2) NOT NULL DEFAULT 100,
  workload_percent numeric(5,2) NOT NULL DEFAULT 0,
  workload_state workload_status NOT NULL DEFAULT 'available',
  assigned_hours numeric(5,2) NOT NULL DEFAULT 0,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  CHECK (availability_percent >= 0 AND availability_percent <= 100),
  CHECK (workload_percent >= 0),
  CHECK (assigned_hours >= 0)
);

CREATE TABLE IF NOT EXISTS skills (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL UNIQUE,
  category skill_category NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS employee_skills (
  employee_id uuid NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
  skill_id uuid NOT NULL REFERENCES skills(id) ON DELETE CASCADE,
  proficiency smallint NOT NULL DEFAULT 3 CHECK (proficiency BETWEEN 1 AND 5),
  is_primary boolean NOT NULL DEFAULT false,
  created_at timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (employee_id, skill_id)
);

CREATE TABLE IF NOT EXISTS projects (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  created_by_user_id uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
  approved_by_user_id uuid REFERENCES app_users(id) ON DELETE SET NULL,
  pm_owner_user_id uuid REFERENCES app_users(id) ON DELETE SET NULL,
  name text NOT NULL,
  client_name text,
  description text,
  notes text,
  start_date date NOT NULL,
  end_date date NOT NULL,
  status project_status NOT NULL DEFAULT 'draft',
  progress_percent integer NOT NULL DEFAULT 0 CHECK (progress_percent BETWEEN 0 AND 100),
  risk_level project_risk_level NOT NULL DEFAULT 'medium',
  resource_utilization_percent numeric(5,2) NOT NULL DEFAULT 0,
  total_required_resources integer NOT NULL DEFAULT 0 CHECK (total_required_resources >= 0),
  submitted_at timestamptz,
  approved_at timestamptz,
  rejected_at timestamptz,
  rejection_reason text,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  CHECK (end_date >= start_date)
);

CREATE INDEX IF NOT EXISTS idx_projects_status ON projects(status);
CREATE INDEX IF NOT EXISTS idx_projects_dates ON projects(start_date, end_date);

CREATE TABLE IF NOT EXISTS project_reviews (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  project_id uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
  reviewer_user_id uuid REFERENCES app_users(id) ON DELETE SET NULL,
  decision review_decision NOT NULL,
  feedback text,
  reviewed_at timestamptz NOT NULL DEFAULT now(),
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_project_reviews_project_id ON project_reviews(project_id);

CREATE TABLE IF NOT EXISTS project_skills (
  project_id uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
  skill_id uuid NOT NULL REFERENCES skills(id) ON DELETE CASCADE,
  created_at timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (project_id, skill_id)
);

CREATE TABLE IF NOT EXISTS project_resource_requirements (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  project_id uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
  role_name text NOT NULL,
  quantity integer NOT NULL DEFAULT 1 CHECK (quantity > 0),
  experience_level experience_level NOT NULL DEFAULT 'mid',
  notes text,
  sort_order integer NOT NULL DEFAULT 0,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_project_requirements_project_id ON project_resource_requirements(project_id);

CREATE TABLE IF NOT EXISTS project_requirement_skills (
  requirement_id uuid NOT NULL REFERENCES project_resource_requirements(id) ON DELETE CASCADE,
  skill_id uuid NOT NULL REFERENCES skills(id) ON DELETE CASCADE,
  created_at timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (requirement_id, skill_id)
);

CREATE TABLE IF NOT EXISTS project_attachments (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  project_id uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
  uploaded_by_user_id uuid REFERENCES app_users(id) ON DELETE SET NULL,
  file_name text NOT NULL,
  storage_key text NOT NULL,
  content_type text,
  file_size_bytes bigint CHECK (file_size_bytes >= 0),
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_project_attachments_project_id ON project_attachments(project_id);

CREATE TABLE IF NOT EXISTS assignments (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  project_id uuid NOT NULL REFERENCES projects(id) ON DELETE CASCADE,
  employee_id uuid NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
  assigned_by_user_id uuid REFERENCES app_users(id) ON DELETE SET NULL,
  role_name text NOT NULL,
  start_date date NOT NULL,
  end_date date NOT NULL,
  allocation_percent numeric(5,2) NOT NULL DEFAULT 0,
  priority priority_level NOT NULL DEFAULT 'medium',
  status assignment_status NOT NULL DEFAULT 'pending',
  progress_percent integer NOT NULL DEFAULT 0 CHECK (progress_percent BETWEEN 0 AND 100),
  conflict_warning text,
  accepted_at timestamptz,
  rejected_at timestamptz,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  CHECK (end_date >= start_date),
  CHECK (allocation_percent >= 0)
);

CREATE INDEX IF NOT EXISTS idx_assignments_project_id ON assignments(project_id);
CREATE INDEX IF NOT EXISTS idx_assignments_employee_id ON assignments(employee_id);
CREATE INDEX IF NOT EXISTS idx_assignments_status ON assignments(status);

CREATE TABLE IF NOT EXISTS assignment_reviews (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  assignment_id uuid NOT NULL UNIQUE REFERENCES assignments(id) ON DELETE CASCADE,
  reviewed_by_user_id uuid REFERENCES app_users(id) ON DELETE SET NULL,
  status assignment_review_status NOT NULL DEFAULT 'pending',
  has_conflict boolean NOT NULL DEFAULT false,
  conflict_details text,
  reviewed_at timestamptz,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_assignment_reviews_status ON assignment_reviews(status);

CREATE TABLE IF NOT EXISTS employee_contracts (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  employee_id uuid NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
  current_project_id uuid REFERENCES projects(id) ON DELETE SET NULL,
  start_date date,
  end_date date,
  status contract_status NOT NULL DEFAULT 'active',
  notes text,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  CHECK (end_date IS NULL OR start_date IS NULL OR end_date >= start_date)
);

CREATE INDEX IF NOT EXISTS idx_employee_contracts_end_date ON employee_contracts(end_date);

CREATE TABLE IF NOT EXISTS gm_decisions (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  project_id uuid REFERENCES projects(id) ON DELETE SET NULL,
  decision_type decision_type NOT NULL,
  title text NOT NULL,
  details text NOT NULL,
  deadline date,
  status decision_status NOT NULL DEFAULT 'pending',
  submitted_by_user_id uuid REFERENCES app_users(id) ON DELETE SET NULL,
  executed_by_user_id uuid REFERENCES app_users(id) ON DELETE SET NULL,
  submitted_at timestamptz NOT NULL DEFAULT now(),
  executed_at timestamptz,
  clarification_requested_at timestamptz,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_gm_decisions_status ON gm_decisions(status);
CREATE INDEX IF NOT EXISTS idx_gm_decisions_type ON gm_decisions(decision_type);

CREATE TABLE IF NOT EXISTS gm_decision_employees (
  decision_id uuid NOT NULL REFERENCES gm_decisions(id) ON DELETE CASCADE,
  employee_id uuid NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
  created_at timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (decision_id, employee_id)
);

CREATE TABLE IF NOT EXISTS notifications (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
  type notification_type NOT NULL,
  title text NOT NULL,
  message text NOT NULL,
  is_read boolean NOT NULL DEFAULT false,
  read_at timestamptz,
  source_entity_type text,
  source_entity_id uuid,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_notifications_user_id_read ON notifications(user_id, is_read);

CREATE TRIGGER trg_departments_updated_at
BEFORE UPDATE ON departments
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_app_users_updated_at
BEFORE UPDATE ON app_users
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_employees_updated_at
BEFORE UPDATE ON employees
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_skills_updated_at
BEFORE UPDATE ON skills
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_projects_updated_at
BEFORE UPDATE ON projects
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_project_reviews_updated_at
BEFORE UPDATE ON project_reviews
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_project_requirements_updated_at
BEFORE UPDATE ON project_resource_requirements
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_project_attachments_updated_at
BEFORE UPDATE ON project_attachments
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_assignments_updated_at
BEFORE UPDATE ON assignments
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_assignment_reviews_updated_at
BEFORE UPDATE ON assignment_reviews
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_employee_contracts_updated_at
BEFORE UPDATE ON employee_contracts
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_gm_decisions_updated_at
BEFORE UPDATE ON gm_decisions
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER trg_notifications_updated_at
BEFORE UPDATE ON notifications
FOR EACH ROW EXECUTE FUNCTION set_updated_at();