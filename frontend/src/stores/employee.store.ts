import { defineStore } from 'pinia';
import { ref } from 'vue';
import { employeeService } from '../services/employee.service';
import type { EmployeeDto, CreateEmployeeRequest, UpdateEmployeeRequest } from '../services/employee.service';

export const useEmployeeStore = defineStore('employee', () => {
    const employees = ref<EmployeeDto[]>([]);
    const isLoading = ref<boolean>(false);
    const error = ref<string | null>(null);

    const fetchEmployees = async () => {
        isLoading.value = true;
        error.value = null;
        try {
            employees.value = await employeeService.getEmployees();
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Có lỗi xảy ra khi tải danh sách nhân viên';
            console.error('Failed to fetch employees:', err);
        } finally {
            isLoading.value = false;
        }
    };

    const createEmployee = async (data: CreateEmployeeRequest) => {
        isLoading.value = true;
        error.value = null;
        try {
            const response = await employeeService.createEmployee(data);
            if (response.success && response.employee) {
                employees.value.unshift(response.employee);
            }
            return response;
        } catch (err: any) {
            const errorMsg = err.response?.data?.message || 'Có lỗi xảy ra khi tạo nhân viên';
            error.value = errorMsg;
            throw err;
        } finally {
            isLoading.value = false;
        }
    };

    const updateEmployee = async (id: string, data: UpdateEmployeeRequest) => {
        isLoading.value = true;
        error.value = null;
        try {
            const response = await employeeService.updateEmployee(id, data);
            if (response.success && response.employee) {
                const index = employees.value.findIndex(e => e.id === id);
                if (index !== -1) {
                    employees.value[index] = response.employee;
                }
            }
            return response;
        } catch (err: any) {
            const errorMsg = err.response?.data?.message || 'Có lỗi xảy ra khi cập nhật nhân viên';
            error.value = errorMsg;
            throw err;
        } finally {
            isLoading.value = false;
        }
    };

    const resendActivation = async (id: string) => {
        isLoading.value = true;
        error.value = null;
        try {
            return await employeeService.resendActivationEmail(id);
        } catch (err: any) {
            const errorMsg = err.response?.data?.message || 'Không thể gửi lại email kích hoạt';
            error.value = errorMsg;
            throw err;
        } finally {
            isLoading.value = false;
        }
    };

    return {
        employees,
        isLoading,
        error,
        fetchEmployees,
        createEmployee,
        updateEmployee,
        resendActivation
    };
});
