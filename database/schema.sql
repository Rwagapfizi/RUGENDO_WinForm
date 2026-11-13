-- RUGENDO schema for MariaDB 10.4+
-- Run:  mariadb -u root -p < database/schema.sql

CREATE DATABASE IF NOT EXISTS rugendo_dot_net CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
CREATE USER IF NOT EXISTS 'rugendo_user'@'localhost' IDENTIFIED BY 'password123';
GRANT ALL PRIVILEGES ON rugendo_dot_net.* TO 'rugendo_user'@'localhost';
FLUSH PRIVILEGES;
USE rugendo_dot_net;

CREATE TABLE IF NOT EXISTS companies (
  id    INT AUTO_INCREMENT PRIMARY KEY,
  name  VARCHAR(100) NOT NULL UNIQUE,
  email VARCHAR(120) NOT NULL,
  phone VARCHAR(20)  NOT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS users (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  full_name     VARCHAR(100) NOT NULL,
  email         VARCHAR(120) NOT NULL UNIQUE,
  phone         VARCHAR(20)  NOT NULL,
  password_hash VARCHAR(100) NOT NULL,
  role          ENUM('CUSTOMER','WORKER') NOT NULL,
  company_id    INT NULL,
  created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_users_company FOREIGN KEY (company_id) REFERENCES companies(id),
  CONSTRAINT chk_worker_company CHECK (role = 'CUSTOMER' OR company_id IS NOT NULL)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS routes (
  id          INT AUTO_INCREMENT PRIMARY KEY,
  company_id  INT NOT NULL,
  origin      VARCHAR(80) NOT NULL,
  destination VARCHAR(80) NOT NULL,
  active      TINYINT(1) NOT NULL DEFAULT 1,
  CONSTRAINT fk_routes_company FOREIGN KEY (company_id) REFERENCES companies(id),
  CONSTRAINT chk_route_diff CHECK (origin <> destination)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_formats (
  id          INT AUTO_INCREMENT PRIMARY KEY,
  company_id  INT NOT NULL,
  name        VARCHAR(60) NOT NULL,
  description VARCHAR(255) NOT NULL DEFAULT '',
  price       DECIMAL(10,2) NOT NULL,
  CONSTRAINT fk_tf_company FOREIGN KEY (company_id) REFERENCES companies(id),
  CONSTRAINT chk_tf_price CHECK (price > 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS schedules (
  id               INT AUTO_INCREMENT PRIMARY KEY,
  route_id         INT NOT NULL,
  ticket_format_id INT NOT NULL,
  departure_date   DATE NOT NULL,
  departure_time   TIME NOT NULL,
  bus_plate        VARCHAR(20) NOT NULL,
  total_seats      INT NOT NULL,
  available_seats  INT NOT NULL,
  CONSTRAINT fk_sch_route  FOREIGN KEY (route_id) REFERENCES routes(id),
  CONSTRAINT fk_sch_format FOREIGN KEY (ticket_format_id) REFERENCES ticket_formats(id),
  CONSTRAINT chk_seats CHECK (total_seats > 0 AND available_seats >= 0 AND available_seats <= total_seats),
  INDEX idx_sch_date (departure_date)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS tickets (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  ticket_code  VARCHAR(20) NOT NULL UNIQUE,
  customer_id  INT NOT NULL,
  schedule_id  INT NOT NULL,
  company_id   INT NOT NULL,
  seat_number  INT NOT NULL,
  price_paid   DECIMAL(10,2) NOT NULL,
  purchased_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  status       ENUM('PAID','CANCELLED') NOT NULL DEFAULT 'PAID',
  CONSTRAINT fk_t_customer FOREIGN KEY (customer_id) REFERENCES users(id),
  CONSTRAINT fk_t_schedule FOREIGN KEY (schedule_id) REFERENCES schedules(id),
  CONSTRAINT fk_t_company  FOREIGN KEY (company_id)  REFERENCES companies(id),
  UNIQUE KEY uq_seat (schedule_id, seat_number)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS receipts (
  id             INT AUTO_INCREMENT PRIMARY KEY,
  ticket_id      INT NOT NULL UNIQUE,
  receipt_number VARCHAR(20) NOT NULL UNIQUE,
  amount         DECIMAL(10,2) NOT NULL,
  issued_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_r_ticket FOREIGN KEY (ticket_id) REFERENCES tickets(id)
) ENGINE=InnoDB;
