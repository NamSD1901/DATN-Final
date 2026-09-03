import api from './api';

export interface EmployeeDto {
    id: string;
    email: string;
    fullName: string;
    phone: string;
    avatar?: string;
    gender?: number;
    dateOfBirth?: string;
    address?: string;
    roleName: string;
    isActive: boolean;
    identityCard: string;
    position: string;
    isResigned: boolean;
}

export interface CreateEmployeeRequest {
    email: string;
    fullName: string;
    phone: string;
    identityCard: string;
    gender?: number;
    dateOfBirth: string;
    address?: string;
    roleName: string;
}

export interface UpdateEmployeeRequest {
    fullName: string;
    phone: string;
    gender?: number;
    dateOfBirth: string;
    address?: string;
    roleName: string;
    isResigned: boolean;
}

export const employeeService = {
    async getEmployees(): Promise<EmployeeDto[]> {
        const response = await api.get('/employees');
        return response.data;
    },

    async createEmployee(data: CreateEmployeeRequest): Promise<{success: boolean, employee?: EmployeeDto, message: string}> {
        const response = await api.post('/employees', data);
        return response.data;
    },

    async updateEmployee(id: string, data: UpdateEmployeeRequest): Promise<{success: boolean, employee?: EmployeeDto, message: string}> {
        const response = await api.put(`/employees/${id}`, data);
        return response.data;
    },

    async resendActivationEmail(id: string): Promise<{success: boolean, message: string}> {
        const response = await api.post(`/employees/${id}/resend-activation`);
        return response.data;
    }
};
